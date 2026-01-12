using QFace.Sdk.RabbitMq.Services;

namespace QimErp.Shared.Common.Services.Workflow;

public class WorkflowApprovalProcessor(
    IRabbitMqPublisher publisher,
    ILogger<WorkflowApprovalProcessor> logger)
    : IWorkflowApprovalProcessor
{
    public async Task ProcessApprovalRequestAsync<TContext>(
        WorkflowApprovalRequestEvent @event,
        TContext context,
        CancellationToken cancellationToken = default)
        where TContext : DbContext
    {
        logger.LogInformation("📨 [WorkflowApprovalProcessor] Processing approval request for WorkflowId={WorkflowId}, EntityType={EntityType}, EntityId={EntityId}",
            @event.WorkflowId, @event.EntityType, @event.EntityId);

        if (string.IsNullOrWhiteSpace(@event.EntityId) || !Guid.TryParse(@event.EntityId, out var entityId))
        {
            logger.LogWarning("⚠️ [WorkflowApprovalProcessor] Missing or invalid EntityId for event. Ignoring message.");
            return;
        }

        if (string.IsNullOrWhiteSpace(@event.EntityType))
        {
            logger.LogWarning("⚠️ [WorkflowApprovalProcessor] Missing EntityType for event. Ignoring message.");
            return;
        }

        var entity = await GetEntityByTypeAsync(context, @event.EntityType, entityId, cancellationToken);

        if (entity == null)
        {
            logger.LogWarning("⚠️ [WorkflowApprovalProcessor] Entity not found with Id={EntityId}, EntityType={EntityType}",
                entityId, @event.EntityType);
            return;
        }

        var workflowCode = @event.WorkflowCode;
        if (string.IsNullOrWhiteSpace(workflowCode))
        {
            workflowCode = entity.WorkflowCode;
        }

        if (string.IsNullOrWhiteSpace(workflowCode))
        {
            logger.LogWarning("⚠️ [WorkflowApprovalProcessor] WorkflowCode is not available for EntityType={EntityType}, EntityId={EntityId}",
                @event.EntityType, entityId);
            return;
        }

        var entityWorkflowStep = await GetEntityWorkflowStepAsync(context, workflowCode, @event.EntityType, cancellationToken);
        if (entityWorkflowStep == null)
        {
            logger.LogWarning("⚠️ [WorkflowApprovalProcessor] No active EntityWorkflowStep found for WorkflowCode={WorkflowCode}, EntityType={EntityType}",
                workflowCode, @event.EntityType);
            return;
        }

        var currentState = @event.CurrentState ?? entity.CurrentWorkflowState;
        var currentStep = GetCurrentWorkflowStep(entityWorkflowStep.WorkflowDefinition, currentState);

        if (currentStep == null)
        {
            logger.LogWarning("⚠️ [WorkflowApprovalProcessor] No workflow step found for current state. WorkflowCode={WorkflowCode}, CurrentState={CurrentState}",
                workflowCode, currentState);
            return;
        }

        var shouldComplete = currentStep.OnApproval.CompleteWorkflow;
        var nextStepCode = currentStep.OnApproval.NextStepCode;

        var oldStatus = entity.WorkflowStatus;
        WorkflowStatus newStatus;

        if (!shouldComplete && !string.IsNullOrWhiteSpace(nextStepCode))
        {
            newStatus = WorkflowStatus.InProgress;
            entity.CurrentWorkflowState = nextStepCode;
            entity.WorkflowStatus = newStatus;
            logger.LogInformation("➡️ Moving {EntityType} {EntityId} to next step: {NextStep}",
                @event.EntityType, entityId, nextStepCode);

            await PublishNotificationsAsync(
                entityWorkflowStep.WorkflowDefinition.Notifications,
                currentStep.OnApproval,
                "Approval",
                @event,
                currentStep.Name);
        }
        else if (shouldComplete)
        {
            newStatus = WorkflowStatus.Approved;
            entity.WorkflowStatus = newStatus;
            entity.CurrentWorkflowState = AppConstant.Workflow.States.Completed;
            entity.WorkflowCompletedAt = @event.ApprovedAt != default ? @event.ApprovedAt : DateTime.UtcNow;
            entity.WorkflowCompletedByEmail = @event.ApprovedBy;
            entity.WorkflowCompletedByEmployeeId = @event.TriggeredBy;
            entity.WorkflowCompletedByName = @event.UserName;
            if (!string.IsNullOrWhiteSpace(@event.Comments))
            {
                entity.WorkflowComments = @event.Comments;
            }
            if (entity is AuditableEntity auditableEntity)
            {
                auditableEntity.ActivateFromDraft();
            }
            logger.LogInformation("✅ Completing workflow for {EntityType} {EntityId} - all steps approved",
                @event.EntityType, entityId);

            await PublishNotificationsAsync(
                entityWorkflowStep.WorkflowDefinition.Notifications,
                currentStep.OnApproval,
                "Completion",
                @event,
                currentStep.Name);
        }
        else
        {
            newStatus = WorkflowStatus.Approved;
            entity.WorkflowStatus = newStatus;
            entity.CurrentWorkflowState = AppConstant.Workflow.States.Completed;
            entity.WorkflowCompletedAt = @event.ApprovedAt != default ? @event.ApprovedAt : DateTime.UtcNow;
            entity.WorkflowCompletedByEmail = @event.ApprovedBy;
            entity.WorkflowCompletedByEmployeeId = @event.TriggeredBy;
            entity.WorkflowCompletedByName = @event.UserName;
            if (!string.IsNullOrWhiteSpace(@event.Comments))
            {
                entity.WorkflowComments = @event.Comments;
            }
            if (entity is AuditableEntity auditableEntity)
            {
                auditableEntity.ActivateFromDraft();
            }
            logger.LogInformation("✅ Completing workflow for {EntityType} {EntityId} - no next step defined",
                @event.EntityType, entityId);

            await PublishNotificationsAsync(
                entityWorkflowStep.WorkflowDefinition.Notifications,
                currentStep.OnApproval,
                "Completion",
                @event,
                currentStep.Name);
        }

        var entry = context.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            context.Update(entity);
        }
        else
        {
            entry.State = EntityState.Modified;
        }

        await context.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("✅ [WorkflowApprovalProcessor] Successfully processed approval request for WorkflowId={WorkflowId}, Status={Status}, CurrentState={CurrentState}",
            @event.WorkflowId, newStatus, entity.CurrentWorkflowState);
    }

    private async Task<IWorkflowEnabled?> GetEntityByTypeAsync<TContext>(
        TContext context,
        string entityTypeName,
        Guid entityId,
        CancellationToken cancellationToken)
        where TContext : DbContext
    {
        try
        {
            var contextType = context.GetType();
            var dbSetProperties = contextType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType.IsGenericType &&
                           p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .ToList();

            Type? entityType = null;
            PropertyInfo? dbSetProperty = null;

            foreach (var prop in dbSetProperties)
            {
                var genericArg = prop.PropertyType.GetGenericArguments()[0];
                if (string.Equals(genericArg.Name, entityTypeName, StringComparison.OrdinalIgnoreCase))
                {
                    entityType = genericArg;
                    dbSetProperty = prop;
                    break;
                }
            }

            if (entityType == null || dbSetProperty == null)
            {
                logger.LogWarning("⚠️ [WorkflowApprovalProcessor] No DbSet found for EntityType={EntityType} in DbContext {ContextType}",
                    entityTypeName, contextType.Name);
                return null;
            }

            if (!typeof(IWorkflowEnabled).IsAssignableFrom(entityType))
            {
                logger.LogWarning("⚠️ [WorkflowApprovalProcessor] EntityType={EntityType} does not implement IWorkflowEnabled",
                    entityTypeName);
                return null;
            }

            var setMethod = typeof(DbContext).GetMethod("Set", Type.EmptyTypes);
            if (setMethod == null)
            {
                logger.LogWarning("⚠️ [WorkflowApprovalProcessor] Could not find Set method on DbContext");
                return null;
            }

            var setGenericMethod = setMethod.MakeGenericMethod(entityType);
            var dbSet = setGenericMethod.Invoke(context, null);
            if (dbSet == null)
            {
                logger.LogWarning("⚠️ [WorkflowApprovalProcessor] Could not get DbSet for EntityType={EntityType}",
                    entityTypeName);
                return null;
            }

            var idProperty = entityType.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
            if (idProperty == null)
            {
                logger.LogWarning("⚠️ [WorkflowApprovalProcessor] EntityType={EntityType} does not have an Id property",
                    entityTypeName);
                return null;
            }

            var parameter = Expression.Parameter(entityType, "e");
            var idPropertyAccess = Expression.Property(parameter, idProperty);
            var idConstant = Expression.Constant(entityId, entityId.GetType());
            var equals = Expression.Equal(idPropertyAccess, idConstant);
            var lambdaType = typeof(Func<,>).MakeGenericType(entityType, typeof(bool));
            var lambda = Expression.Lambda(lambdaType, equals, parameter);

            var whereMethod = typeof(Queryable).GetMethods()
                .FirstOrDefault(m => m.Name == "Where" && m.GetParameters().Length == 2);
            if (whereMethod == null)
            {
                logger.LogWarning("⚠️ [WorkflowApprovalProcessor] Could not find Where method");
                return null;
            }

            var whereGenericMethod = whereMethod.MakeGenericMethod(entityType);
            var queryable = dbSet as IQueryable;
            if (queryable == null)
            {
                logger.LogWarning("⚠️ [WorkflowApprovalProcessor] DbSet is not IQueryable");
                return null;
            }

            var filteredQuery = whereGenericMethod.Invoke(null, new object[] { queryable, lambda });
            if (filteredQuery == null)
            {
                return null;
            }

            var firstOrDefaultAsyncMethod = typeof(EntityFrameworkQueryableExtensions)
                .GetMethods()
                .FirstOrDefault(m => m.Name == "FirstOrDefaultAsync" && 
                                    m.GetParameters().Length == 2 &&
                                    m.GetParameters()[1].ParameterType == typeof(CancellationToken));
            
            if (firstOrDefaultAsyncMethod == null)
            {
                logger.LogWarning("⚠️ [WorkflowApprovalProcessor] Could not find FirstOrDefaultAsync method");
                return null;
            }

            var firstOrDefaultGenericMethod = firstOrDefaultAsyncMethod.MakeGenericMethod(entityType);
            var task = firstOrDefaultGenericMethod.Invoke(null, new object[] { filteredQuery, cancellationToken });
            
            if (task == null)
            {
                return null;
            }

            await ((Task)task).ConfigureAwait(false);
            var resultProperty = task.GetType().GetProperty("Result");
            var result = resultProperty?.GetValue(task);

            return result as IWorkflowEnabled;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ [WorkflowApprovalProcessor] Error retrieving entity by type. EntityType={EntityType}, EntityId={EntityId}",
                entityTypeName, entityId);
            return null;
        }
    }

    private async Task<EntityWorkflowStep?> GetEntityWorkflowStepAsync<TContext>(
        TContext context,
        string workflowCode,
        string entityType,
        CancellationToken cancellationToken)
        where TContext : DbContext
    {
        try
        {
            var query = context.Set<EntityWorkflowStep>()
                .AsNoTracking()
                .Where(e => e.WorkflowCode == workflowCode &&
                           e.EntityType == entityType &&
                           e.IsActive &&
                           e.DataStatus == DataState.Active);

            return await query.FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "⚠️ [WorkflowApprovalProcessor] Error querying EntityWorkflowSteps from DbContext. EntityWorkflowSteps table may not exist in this context.");
            return null;
        }
    }

    private WorkflowStep? GetCurrentWorkflowStep(WorkflowDefinition workflowDefinition, string? currentState)
    {
        if (string.IsNullOrWhiteSpace(currentState))
        {
            return workflowDefinition.Steps.MinBy(s => s.Order);
        }

        return workflowDefinition.Steps.FirstOrDefault(s => s.StepCode == currentState)
               ?? workflowDefinition.Steps.MinBy(s => s.Order);
    }

    private async Task PublishNotificationsAsync(
        WorkflowNotificationSettings notifications,
        WorkflowStepAction action,
        string notificationType,
        WorkflowApprovalRequestEvent @event,
        string stepName)
    {
        if (notifications.SendSmsNotifications && action.SendNotificationTo.Count > 0)
        {
            var smsMessage = BuildSmsNotification(@event, notificationType, stepName, action.SendNotificationTo);
            await publisher.PublishAsync(smsMessage, "qimerp.core.notify.prod_exchange");
            logger.LogInformation("📱 [WorkflowNotification] Published SMS notification for {NotificationType} to {RecipientCount} recipients",
                notificationType, action.SendNotificationTo.Count);
        }

        if (notifications.SendEmailNotifications)
        {
            var recipients = action.SendEmailTo
                .Concat(GetRecipientsForType(notifications, notificationType))
                .Distinct()
                .ToList();

            if (recipients.Count > 0)
            {
                var emailMessage = BuildEmailNotification(@event, notificationType, stepName, recipients);
                await publisher.PublishAsync(emailMessage, "qimerp.core.notify.prod_exchange");
                logger.LogInformation("📧 [WorkflowNotification] Published Email notification for {NotificationType} to {RecipientCount} recipients",
                    notificationType, recipients.Count);
            }
        }
    }

    private UnifiedMessageModel BuildSmsNotification(
        WorkflowApprovalRequestEvent @event,
        string notificationType,
        string stepName,
        List<string> phoneNumbers)
    {
        return new UnifiedMessageModel
        {
            MessageType = phoneNumbers.Count > 1 ? "bulk_sms" : "sms",
            PhoneNumber = phoneNumbers.Count == 1 ? phoneNumbers[0] : null,
            PhoneNumbers = phoneNumbers.Count > 1 ? phoneNumbers : null,
            Message = $"Workflow {notificationType}: {@event.EntityType} at step {stepName}. Please review.",
            MessageId = Guid.NewGuid().ToString(),
            CorrelationId = @event.WorkflowId,
            Metadata = new Dictionary<string, string>
            {
                ["SourceModule"] = "Workflow",
                ["SourceEntityType"] = @event.EntityType,
                ["SourceEntityId"] = @event.EntityId,
                ["NotificationType"] = notificationType
            }
        };
    }

    private static UnifiedMessageModel BuildEmailNotification(
        WorkflowApprovalRequestEvent @event,
        string notificationType,
        string stepName,
        List<string> emails)
    {
        return new UnifiedMessageModel
        {
            MessageType = "templated_email",
            ToEmails = emails,
            Subject = $"Workflow {notificationType}: {@event.EntityType}",
            Template = GetEmailTemplate(notificationType),
            Replacements = new Dictionary<string, string>
            {
                ["EntityType"] = @event.EntityType,
                ["EntityId"] = @event.EntityId,
                ["StepName"] = stepName,
                ["NotificationType"] = notificationType,
                ["Comments"] = @event.Comments ?? "",
                ["ActorName"] = @event.UserName ?? "",
                ["ActorEmail"] = @event.ApprovedBy ?? ""
            },
            MessageId = Guid.NewGuid().ToString(),
            CorrelationId = @event.WorkflowId,
            Metadata = new Dictionary<string, string>
            {
                ["SourceModule"] = "Workflow",
                ["SourceEntityType"] = @event.EntityType,
                ["SourceEntityId"] = @event.EntityId,
                ["NotificationType"] = notificationType
            }
        };
    }

    private static List<string> GetRecipientsForType(WorkflowNotificationSettings settings, string type)
    {
        return type switch
        {
            "Approval" => settings.OnApproval,
            "Rejection" => settings.OnRejection,
            "Completion" => settings.OnCompletion,
            "Timeout" => settings.OnTimeout,
            "OnStart" => settings.OnStart,
            _ => []
        };
    }

    private static string GetEmailTemplate(string notificationType)
    {
        return notificationType switch
        {
            "Approval" => @"
                <html>
                <body>
                    <h2>Workflow Step Approved</h2>
                    <p><strong>Entity Type:</strong> {{EntityType}}</p>
                    <p><strong>Entity ID:</strong> {{EntityId}}</p>
                    <p><strong>Step:</strong> {{StepName}}</p>
                    <p><strong>Approved By:</strong> {{ActorName}} ({{ActorEmail}})</p>
                    <p><strong>Comments:</strong> {{Comments}}</p>
                </body>
                </html>",
            "Rejection" => @"
                <html>
                <body>
                    <h2>Workflow Step Rejected</h2>
                    <p><strong>Entity Type:</strong> {{EntityType}}</p>
                    <p><strong>Entity ID:</strong> {{EntityId}}</p>
                    <p><strong>Step:</strong> {{StepName}}</p>
                    <p><strong>Rejected By:</strong> {{ActorName}} ({{ActorEmail}})</p>
                    <p><strong>Reason:</strong> {{Comments}}</p>
                </body>
                </html>",
            "Completion" => @"
                <html>
                <body>
                    <h2>Workflow Completed</h2>
                    <p><strong>Entity Type:</strong> {{EntityType}}</p>
                    <p><strong>Entity ID:</strong> {{EntityId}}</p>
                    <p><strong>Final Step:</strong> {{StepName}}</p>
                    <p><strong>Completed By:</strong> {{ActorName}} ({{ActorEmail}})</p>
                    <p>The workflow has been fully approved and completed.</p>
                </body>
                </html>",
            "Timeout" => @"
                <html>
                <body>
                    <h2>Workflow Timeout Warning</h2>
                    <p><strong>Entity Type:</strong> {{EntityType}}</p>
                    <p><strong>Entity ID:</strong> {{EntityId}}</p>
                    <p><strong>Step:</strong> {{StepName}}</p>
                    <p>This workflow step has timed out and requires attention.</p>
                </body>
                </html>",
            _ => @"
                <html>
                <body>
                    <h2>Workflow Notification</h2>
                    <p><strong>Entity Type:</strong> {{EntityType}}</p>
                    <p><strong>Entity ID:</strong> {{EntityId}}</p>
                    <p><strong>Step:</strong> {{StepName}}</p>
                    <p><strong>Type:</strong> {{NotificationType}}</p>
                </body>
                </html>"
        };
    }
}

