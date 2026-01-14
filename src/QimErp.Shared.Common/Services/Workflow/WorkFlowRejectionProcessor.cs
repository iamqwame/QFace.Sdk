using QFace.Sdk.RabbitMq.Services;

namespace QimErp.Shared.Common.Services.Workflow;

/// <summary>
/// Implements the workflow rejection processor to handle rejection requests.
/// </summary>
/// <param name="publisher"></param>
/// <param name="templateService"></param>
/// <param name="configuration"></param>
/// <param name="logger"></param>
public class WorkflowRejectionProcessor(
    IRabbitMqPublisher publisher,
    ITemplateService templateService,
    IConfiguration configuration,
    ILogger<WorkflowRejectionProcessor> logger)
    : IWorkflowRejectionProcessor
{
    /// <summary>
    /// Processes a workflow rejection request.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TContext"></typeparam>
    public async Task ProcessRejectionRequestAsync<TContext>(
        WorkflowRejectionRequestEvent @event,
        TContext context,
        CancellationToken cancellationToken = default)
        where TContext : DbContext
    {
        logger.LogInformation("📨 [WorkflowRejectionProcessor] Processing rejection request for WorkflowId={WorkflowId}, EntityType={EntityType}, EntityId={EntityId}",
            @event.WorkflowId, @event.EntityType, @event.EntityId);

        if (string.IsNullOrWhiteSpace(@event.EntityId) || !Guid.TryParse(@event.EntityId, out var entityId))
        {
            logger.LogWarning("⚠️ [WorkflowRejectionProcessor] Missing or invalid EntityId for event. Ignoring message.");
            return;
        }

        if (string.IsNullOrWhiteSpace(@event.EntityType))
        {
            logger.LogWarning("⚠️ [WorkflowRejectionProcessor] Missing EntityType for event. Ignoring message.");
            return;
        }

        var entity = await GetEntityByTypeAsync(context, @event.EntityType, entityId, cancellationToken);

        if (entity == null)
        {
            logger.LogWarning("⚠️ [WorkflowRejectionProcessor] Entity not found with Id={EntityId}, EntityType={EntityType}",
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
            logger.LogWarning("⚠️ [WorkflowRejectionProcessor] WorkflowCode is not available for EntityType={EntityType}, EntityId={EntityId}",
                @event.EntityType, entityId);
            return;
        }

        var entityWorkflowStep = await GetEntityWorkflowStepAsync(context, workflowCode, @event.EntityType, cancellationToken);
        if (entityWorkflowStep == null)
        {
            logger.LogWarning("⚠️ [WorkflowRejectionProcessor] No active EntityWorkflowStep found for WorkflowCode={WorkflowCode}, EntityType={EntityType}",
                workflowCode, @event.EntityType);
            return;
        }

        var currentState = @event.CurrentState;
        var currentStep = GetCurrentWorkflowStep(entityWorkflowStep.WorkflowDefinition, currentState);

        if (currentStep == null)
        {
            logger.LogWarning("⚠️ [WorkflowRejectionProcessor] No workflow step found for current state. WorkflowCode={WorkflowCode}, CurrentState={CurrentState}",
                workflowCode, currentState);
            return;
        }

        entity.RejectWorkflow(
            @event.RejectedBy,
            @event.TriggeredBy,
            @event.UserName,
            @event.RejectionReason);

        entity.CurrentWorkflowState = currentState ?? "Rejected";

        logger.LogInformation("❌ Rejecting workflow for {EntityType} {EntityId} at step {StepName}",
            @event.EntityType, entityId, currentStep.Name);

        await PublishRejectionNotificationsAsync(
            entityWorkflowStep.WorkflowDefinition.Notifications,
            currentStep.OnRejection,
            @event,
            currentStep.Name,
            entity);

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

        logger.LogInformation("✅ [WorkflowRejectionProcessor] Successfully processed rejection request for WorkflowId={WorkflowId}, Status=Rejected, CurrentState={CurrentState}",
            @event.WorkflowId, entity.CurrentWorkflowState);
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
                logger.LogWarning("⚠️ [WorkflowRejectionProcessor] No DbSet found for EntityType={EntityType} in DbContext {ContextType}",
                    entityTypeName, contextType.Name);
                return null;
            }

            if (!typeof(IWorkflowEnabled).IsAssignableFrom(entityType))
            {
                logger.LogWarning("⚠️ [WorkflowRejectionProcessor] EntityType={EntityType} does not implement IWorkflowEnabled",
                    entityTypeName);
                return null;
            }

            var setMethod = typeof(DbContext).GetMethod("Set", Type.EmptyTypes);
            if (setMethod == null)
            {
                logger.LogWarning("⚠️ [WorkflowRejectionProcessor] Could not find Set method on DbContext");
                return null;
            }

            var setGenericMethod = setMethod.MakeGenericMethod(entityType);
            var dbSet = setGenericMethod.Invoke(context, null);
            if (dbSet == null)
            {
                logger.LogWarning("⚠️ [WorkflowRejectionProcessor] Could not get DbSet for EntityType={EntityType}",
                    entityTypeName);
                return null;
            }

            var idProperty = entityType.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
            if (idProperty == null)
            {
                logger.LogWarning("⚠️ [WorkflowRejectionProcessor] EntityType={EntityType} does not have an Id property",
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
                logger.LogWarning("⚠️ [WorkflowRejectionProcessor] Could not find Where method");
                return null;
            }

            var whereGenericMethod = whereMethod.MakeGenericMethod(entityType);
            var queryable = dbSet as IQueryable;
            if (queryable == null)
            {
                logger.LogWarning("⚠️ [WorkflowRejectionProcessor] DbSet is not IQueryable");
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
                logger.LogWarning("⚠️ [WorkflowRejectionProcessor] Could not find FirstOrDefaultAsync method");
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
            logger.LogError(ex, "❌ [WorkflowRejectionProcessor] Error retrieving entity by type. EntityType={EntityType}, EntityId={EntityId}",
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
            logger.LogWarning(ex, "⚠️ [WorkflowRejectionProcessor] Error querying EntityWorkflowSteps from DbContext. EntityWorkflowSteps table may not exist in this context.");
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

    private async Task PublishRejectionNotificationsAsync(
        WorkflowNotificationSettings notifications,
        WorkflowStepAction? action,
        WorkflowRejectionRequestEvent @event,
        string stepName,
        IWorkflowEnabled entity)
    {
        if (notifications == null || !notifications.SendEmailNotifications)
        {
            logger.LogDebug("📧 [WorkflowRejectionProcessor] Email notifications are disabled. Skipping rejection notification sending.");
            return;
        }

        var recipients = new List<string>();

        if (action?.SendEmailTo != null && action.SendEmailTo.Count > 0)
        {
            recipients.AddRange(action.SendEmailTo);
        }

        if (notifications.OnRejection.Count > 0)
        {
            recipients.AddRange(notifications.OnRejection);
        }

        if (@event.ReturnToOriginator && !string.IsNullOrWhiteSpace(entity.WorkflowInitiatedByEmail))
        {
            recipients.Add(entity.WorkflowInitiatedByEmail);
        }

        recipients = recipients.Distinct(StringComparer.OrdinalIgnoreCase).Where(r => !string.IsNullOrWhiteSpace(r)).ToList();

        if (recipients.Count == 0)
        {
            logger.LogDebug("📧 [WorkflowRejectionProcessor] No recipients found for rejection notifications.");
            return;
        }

        logger.LogInformation("📧 [WorkflowRejectionProcessor] Sending rejection notifications to {RecipientCount} recipients",
            recipients.Count);

        var entityDisplayName = GetEntityDisplayName(entity);
        var rejectedByName = @event.UserName ?? FormatEmailAsName(@event.RejectedBy) ?? "System";
        var rejectedByEmail = @event.RejectedBy ?? "system@qimerp.com";
        var rejectedAt = @event.RejectedAt != default ? @event.RejectedAt : DateTime.UtcNow;
        var baseUrl = configuration.GetValue<string>("FrontendSettings:BaseUrl") 
            ?? configuration.GetValue<string>("FrontendSettings__BaseUrl")
            ?? "https://app.qimerp.com";
        var reviewUrl = $"{baseUrl.TrimEnd('/')}/workflow/entity/{@event.EntityType}/{@event.EntityId}/review";

        var replacements = new Dictionary<string, string>
        {
            ["EntityType"] = @event.EntityType ?? "",
            ["EntityName"] = entityDisplayName,
            ["StepName"] = stepName ?? "",
            ["Comments"] = @event.RejectionReason ?? "",
            ["ActorName"] = rejectedByName,
            ["ApproverName"] = rejectedByName,
            ["ActorEmail"] = rejectedByEmail,
            ["WorkflowCode"] = @event.WorkflowCode ?? entity.WorkflowCode?.Replace("-", " ") ?? "Workflow Request",
            ["RequesterName"] = entity.WorkflowInitiatedByName ?? FormatEmailAsName(entity.WorkflowInitiatedByEmail) ?? "Requester",
            ["Date"] = rejectedAt.ToString("MMMM dd, yyyy"),
            ["ReviewUrl"] = reviewUrl,
            ["Year"] = DateTime.UtcNow.Year.ToString()
        };

        var emailTemplate = await templateService.RenderEmailTemplateAsync("WorkflowRejection", replacements);

        foreach (var recipient in recipients)
        {
            try
            {
                var message = new UnifiedMessageModel
                {
                    MessageType = "templated_email",
                    ToEmail = recipient,
                    Subject = $"Request Needs Attention: {entityDisplayName} - Rejected",
                    Template = emailTemplate,
                    Replacements = replacements,
                    MessageId = Guid.NewGuid().ToString(),
                    CorrelationId = @event.WorkflowId,
                    Metadata = new Dictionary<string, string>
                    {
                        ["SourceModule"] = "Workflow",
                        ["SourceEntityType"] = @event.EntityType ?? "",
                        ["SourceEntityId"] = @event.EntityId ?? "",
                        ["NotificationType"] = "WorkflowRejection"
                    }
                };

                await publisher.PublishAsync(message, "qimerp.core.notify.prod_exchange");

                logger.LogInformation("✅ [WorkflowRejectionProcessor] Successfully sent rejection notification to {Recipient}",
                    recipient);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ [WorkflowRejectionProcessor] Failed to send rejection notification to {Recipient}",
                    recipient);
            }
        }
    }

    private static string GetEntityDisplayName(IWorkflowEnabled entity)
    {
        var entityType = entity.GetType();
        var nameProperty = entityType.GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
        if (nameProperty != null)
        {
            var nameValue = nameProperty.GetValue(entity);
            if (nameValue != null && !string.IsNullOrWhiteSpace(nameValue.ToString()))
            {
                return nameValue.ToString()!;
            }
        }

        var titleProperty = entityType.GetProperty("Title", BindingFlags.Public | BindingFlags.Instance);
        if (titleProperty != null)
        {
            var titleValue = titleProperty.GetValue(entity);
            if (titleValue != null && !string.IsNullOrWhiteSpace(titleValue.ToString()))
            {
                return titleValue.ToString()!;
            }
        }

        return entity.EntityType;
    }

    private static string? FormatEmailAsName(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        if (!email.Contains('@'))
            return null;

        var localPart = email.Split('@')[0];

        var formatted = localPart
            .Replace('.', ' ')
            .Replace('_', ' ')
            .Replace('-', ' ');

        var words = formatted.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var capitalized = string.Join(" ", words.Select(w =>
            w.Length > 0 ? char.ToUpperInvariant(w[0]) + w.Substring(1).ToLowerInvariant() : w));

        return capitalized;
    }
}
