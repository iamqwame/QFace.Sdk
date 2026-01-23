using QFace.Sdk.RabbitMq.Services;

namespace QimErp.Shared.Common.Services.Workflow;

/// <summary>
/// Processor for handling workflow approval requests.
/// </summary>
/// <param name="publisher"></param>
/// <param name="templateService"></param>
/// <param name="configuration"></param>
/// <param name="dynamicHtmlGenerator"></param>
/// <param name="logger"></param>
public class WorkflowApprovalProcessor(
    IRabbitMqPublisher publisher,
    ITemplateService templateService,
    IConfiguration configuration,
    IDynamicHtmlGenerator dynamicHtmlGenerator,
    ILogger<WorkflowApprovalProcessor> logger)
    : IWorkflowApprovalProcessor
{
    /// <summary>
    /// Processes a workflow approval request event.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TContext"></typeparam>
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

        var workflowName = @event.WorkflowCode.Replace("-", " ");
        @event.WorkflowCode = workflowName;
        
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

        var approvedStepCode = !string.IsNullOrWhiteSpace(@event.PreviousStep) 
            ? @event.PreviousStep 
            : @event.CurrentState;
        
        var approvedStep = GetCurrentWorkflowStep(entityWorkflowStep.WorkflowDefinition, approvedStepCode);
        if (approvedStep == null)
        {
            logger.LogWarning("⚠️ [WorkflowApprovalProcessor] No workflow step found for approved step. WorkflowCode={WorkflowCode}, ApprovedStepCode={ApprovedStepCode}",
                workflowCode, approvedStepCode);
            return;
        }

        var shouldCompleteStep = @event.ShouldComplete;
        var nextStepCode = @event.NextStepCode;

        WorkflowStatus newStatus;

        if (@event.IsLastStep && shouldCompleteStep)
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
            logger.LogInformation("✅ Completing workflow for {EntityType} {EntityId} - all steps approved. Approved step: {ApprovedStep}",
                @event.EntityType, entityId, approvedStepCode);

            await PublishNotificationsAsync(
                entityWorkflowStep.WorkflowDefinition.Notifications,
                approvedStep.OnApproval,
                "Completion",
                @event,
                approvedStep.Name,
                entity,
                entityWorkflowStep.WorkflowDefinition);

            await SendRequesterNotificationAsync(@event, entity, "WorkflowCompleted", approvedStep.Name);
        }
        else if (!string.IsNullOrWhiteSpace(nextStepCode))
        {
            newStatus = WorkflowStatus.InProgress;
            entity.CurrentWorkflowState = nextStepCode;
            entity.WorkflowStatus = newStatus;
            logger.LogInformation("➡️ Moving {EntityType} {EntityId} to next step: {NextStep}. Approved step: {ApprovedStep}",
                @event.EntityType, entityId, nextStepCode, approvedStepCode);

            await PublishNotificationsAsync(
                entityWorkflowStep.WorkflowDefinition.Notifications,
                approvedStep.OnApproval,
                "StepApproved",
                @event,
                approvedStep.Name,
                entity,
                entityWorkflowStep.WorkflowDefinition);

            await SendRequesterNotificationAsync(@event, entity, "StepApproved", approvedStep.Name);

            var nextStep = GetNextWorkflowStep(entityWorkflowStep.WorkflowDefinition, nextStepCode);
            if (nextStep != null && nextStep.RequiredApprovers.Count > 0)
            {
                logger.LogDebug("📧 [WorkflowApprovalProcessor] Notifying required approvers for next step: {StepCode}",
                    nextStepCode);
                await SendNextStepNotificationsAsync(
                    entityWorkflowStep.WorkflowDefinition,
                    nextStep,
                    @event,
                    entity);
            }
        }
        else if (shouldCompleteStep)
        {
            newStatus = WorkflowStatus.InProgress;
            entity.WorkflowStatus = newStatus;
            entity.CurrentWorkflowState = @event.CurrentState;
            logger.LogInformation("✅ Step completed for {EntityType} {EntityId} - no next step defined. Approved step: {ApprovedStep}",
                @event.EntityType, entityId, approvedStepCode);

            await PublishNotificationsAsync(
                entityWorkflowStep.WorkflowDefinition.Notifications,
                approvedStep.OnApproval,
                "StepApproved",
                @event,
                approvedStep.Name,
                entity,
                entityWorkflowStep.WorkflowDefinition);

            await SendRequesterNotificationAsync(@event, entity, "StepApproved", approvedStep.Name);
        }
        else
        {
            newStatus = WorkflowStatus.InProgress;
            entity.WorkflowStatus = newStatus;
            entity.CurrentWorkflowState = @event.CurrentState;
            logger.LogInformation("✅ Step approved for {EntityType} {EntityId} - no transition defined. Approved step: {ApprovedStep}",
                @event.EntityType, entityId, approvedStepCode);

            await PublishNotificationsAsync(
                entityWorkflowStep.WorkflowDefinition.Notifications,
                approvedStep.OnApproval,
                "Approval",
                @event,
                approvedStep.Name,
                entity,
                entityWorkflowStep.WorkflowDefinition);

            await SendRequesterNotificationAsync(@event, entity, "StepApproved", approvedStep.Name);
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

    private WorkflowStep? GetNextWorkflowStep(WorkflowDefinition workflowDefinition, string nextStepCode)
    {
        if (string.IsNullOrWhiteSpace(nextStepCode))
        {
            return null;
        }

        return workflowDefinition.Steps.FirstOrDefault(s => s.StepCode == nextStepCode);
    }

    private async Task PublishNotificationsAsync(
        WorkflowNotificationSettings? notifications,
        WorkflowStepAction? action,
        string notificationType,
        WorkflowApprovalRequestEvent @event,
        string stepName,
        IWorkflowEnabled entity,
        WorkflowDefinition? workflowDefinition = null)
    {
        if (notifications == null || action == null)
        {
            logger.LogDebug("📧 [WorkflowNotification] Notifications or action is null. Skipping notification publishing.");
            return;
        }

        if (notifications.SendSmsNotifications && action.SendNotificationTo.Count > 0)
        {
            try
            {
                var smsMessage = BuildSmsNotification(@event, notificationType, stepName, action.SendNotificationTo);
                await publisher.PublishAsync(smsMessage, "qimerp.core.notify.prod_exchange");
                logger.LogInformation("📱 [WorkflowNotification] Published SMS notification for {NotificationType} to {RecipientCount} recipients",
                    notificationType, action.SendNotificationTo.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ [WorkflowNotification] Failed to publish SMS notification for {NotificationType}. Continuing with email notifications.",
                    notificationType);
            }
        }

        if (notifications.SendEmailNotifications)
        {
            try
            {
                var recipients = new List<string>();
                
                if (action.SendEmailTo.Count > 0)
                {
                    recipients.AddRange(action.SendEmailTo);
                }
                
                var typeRecipients = GetRecipientsForType(notifications, notificationType);
                if (typeRecipients.Count > 0)
                {
                    var validTypeRecipients = typeRecipients
                        .Where(r => !string.IsNullOrWhiteSpace(r) && IsValidEmail(r))
                        .ToList();
                    
                    if (validTypeRecipients.Count > 0)
                    {
                        recipients.AddRange(validTypeRecipients);
                    }
                }
                
                recipients = recipients.Distinct().ToList();

                if (recipients.Count > 0)
                {
                    var emailMessage = await BuildEmailNotificationAsync(@event, notificationType, stepName, recipients, entity, workflowDefinition);
                    await publisher.PublishAsync(emailMessage, "qimerp.core.notify.prod_exchange");
                    logger.LogInformation("📧 [WorkflowNotification] Published Email notification for {NotificationType} to {RecipientCount} recipients",
                        notificationType, recipients.Count);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ [WorkflowNotification] Failed to publish Email notification for {NotificationType}",
                    notificationType);
            }
        }
    }

    private static UnifiedMessageModel BuildSmsNotification(
        WorkflowApprovalRequestEvent @event,
        string notificationType,
        string stepName,
        List<string> phoneNumbers)
    {
        return new UnifiedMessageModel
        {
            MessageType = phoneNumbers.Count > 1 ? "bulk_sms" : "sms",
            PhoneNumber = phoneNumbers.Count == 1 ? phoneNumbers[0] : string.Empty,
            PhoneNumbers = phoneNumbers.Count > 1 ? phoneNumbers : [],
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

    private async Task<UnifiedMessageModel> BuildEmailNotificationAsync(
        WorkflowApprovalRequestEvent @event,
        string notificationType,
        string stepName,
        List<string> emails,
        IWorkflowEnabled entity,
        WorkflowDefinition? workflowDefinition = null)
    {
        var templateName = notificationType switch
        {
            "Approval" => "WorkflowApproval",
            "Rejection" => "WorkflowRejection",
            "Completion" => "WorkflowCompletion",
            "Timeout" => "WorkflowTimeout",
            _ => "WorkflowApproval"
        };

        workflowDefinition ??= entity.WorkflowDefinition;
        
        var entityDetails = ExtractEntityDetails(entity);
        var entityDisplayName = GetEntityDisplayName(entity);
        var baseUrl = configuration.GetValue<string>("FrontendSettings:BaseUrl") 
            ?? configuration.GetValue<string>("FrontendSettings__BaseUrl")
            ?? "https://app.qimerp.com";
        var reviewUrl = $"{baseUrl.TrimEnd('/')}/workflow/entity/{@event.EntityType}/{@event.EntityId}/review";
        var approvedDate = @event.ApprovedAt != default ? @event.ApprovedAt : DateTime.UtcNow;
        var initiatedAt = entity.WorkflowInitiatedAt ?? DateTime.UtcNow;

        var currentStep = workflowDefinition.Steps.FirstOrDefault(s => s.Name == stepName);
        var currentStepNumber = currentStep?.Order ?? 0;
        var totalSteps = workflowDefinition.Steps?.Count ?? 0;
        var stageInfo = totalSteps > 0 && currentStepNumber > 0
            ? $"Stage {currentStepNumber} of {totalSteps}: Action Required"
            : "Action Required";

        var nextStepName = "Final Review";
        if (!string.IsNullOrWhiteSpace(@event.NextStepCode) && workflowDefinition.Steps != null)
        {
            var nextStep = workflowDefinition.Steps.FirstOrDefault(s => s.StepCode == @event.NextStepCode);
            if (nextStep != null)
            {
                nextStepName = nextStep.Name;
            }
        }
        else if (currentStepNumber > 0 && totalSteps > currentStepNumber && workflowDefinition?.Steps != null)
        {
            var nextStep = workflowDefinition.Steps.OrderBy(s => s.Order).Skip(currentStepNumber).FirstOrDefault();
            if (nextStep != null)
            {
                nextStepName = nextStep.Name;
            }
        }

        var entityReference = entityDetails.TryGetValue("Code", out var code) && !string.IsNullOrWhiteSpace(code)
            ? code
            : entityDetails.TryGetValue("EmployeeNumber", out var empNo) && !string.IsNullOrWhiteSpace(empNo)
                ? empNo
                : @event.EntityId;

        var requesterDisplayName = entityDetails.TryGetValue("FullName", out var fullName) && !string.IsNullOrWhiteSpace(fullName)
            ? fullName
            : entity.WorkflowInitiatedByName ?? FormatEmailAsName(entity.WorkflowInitiatedByEmail) ?? "Requester";

        var requesterLabel = entityDetails.ContainsKey("FullName") ? "Employee Name" : "Requester Name";

        var currentStepCode = @event.CurrentState ?? @event.NextStepCode ?? "";
        if (string.IsNullOrWhiteSpace(currentStepCode) && workflowDefinition?.Steps != null)
        {
            var firstStep = workflowDefinition.Steps.OrderBy(s => s.Order).FirstOrDefault();
            currentStepCode = firstStep?.StepCode ?? "";
        }

        var workflowProgressHtml = workflowDefinition != null
            ? dynamicHtmlGenerator.GenerateWorkflowProgressHtml(workflowDefinition, currentStepCode, initiatedAt, isCompleted: false)
            : dynamicHtmlGenerator.GenerateEmptyProgressHtml(initiatedAt);

        var replacements = new Dictionary<string, string>
        {
            ["EntityType"] = @event.EntityType,
            ["EntityName"] = entityDisplayName,
            ["StepName"] = stepName,
            ["NotificationType"] = notificationType,
            ["Comments"] = @event.Comments,
            ["ActorName"] = @event.UserName ?? "Approver",
            ["ApproverName"] = @event.UserName ?? "Approver",
            ["ActorEmail"] = @event.ApprovedBy,
            ["WorkflowCode"] = @event.WorkflowCode,
            ["RequesterName"] = entity.WorkflowInitiatedByName ?? FormatEmailAsName(entity.WorkflowInitiatedByEmail) ?? "Requester",
            ["Date"] = approvedDate.ToString("MMMM dd, yyyy"),
            ["ReviewUrl"] = reviewUrl,
            ["Year"] = DateTime.UtcNow.Year.ToString(),
            ["EntityReference"] = entityReference,
            ["StageInfo"] = stageInfo,
            ["InitiatedAt"] = initiatedAt.ToString("MMMM dd, yyyy"),
            ["NextStepName"] = nextStepName,
            ["RequesterDisplayName"] = requesterDisplayName,
            ["RequesterLabel"] = requesterLabel,
            ["WorkflowProgressHtml"] = workflowProgressHtml
        };

        foreach (var detail in entityDetails)
        {
            replacements[detail.Key] = detail.Value;
        }

        var emailTemplate = await templateService.RenderEmailTemplateAsync(templateName, replacements);

        return new UnifiedMessageModel
        {
            MessageType = "templated_email",
            ToEmails = emails,
            Subject = $"Workflow {notificationType}: {entityDisplayName}",
            Template = emailTemplate,
            Replacements = replacements,
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

    private async Task SendNextStepNotificationsAsync(
        WorkflowDefinition workflowDefinition,
        WorkflowStep nextStep,
        WorkflowApprovalRequestEvent @event,
        IWorkflowEnabled entity)
    {
        var notifications = workflowDefinition.Notifications;
        if (!notifications.SendEmailNotifications)
        {
            logger.LogDebug("📧 [SendNextStepNotifications] Email notifications are disabled. Skipping next step notification sending.");
            return;
        }

        var recipients = new List<string>();

        if (nextStep.RequiredApprovers.Count > 0)
        {
            var approverEmails = ExtractEmailsFromApprovers(nextStep.RequiredApprovers);
            if (approverEmails.Count > 0)
            {
                recipients.AddRange(approverEmails);
                logger.LogInformation("📧 [SendNextStepNotifications] Extracted {Count} valid emails from {TotalCount} required approvers for step {StepCode}",
                    approverEmails.Count, nextStep.RequiredApprovers.Count, nextStep.StepCode);
            }
            else
            {
                logger.LogDebug("📧 [SendNextStepNotifications] No valid emails found in {TotalCount} required approvers for step {StepCode}",
                    nextStep.RequiredApprovers.Count, nextStep.StepCode);
            }
        }

        recipients = recipients.Distinct(StringComparer.OrdinalIgnoreCase).Where(r => !string.IsNullOrWhiteSpace(r) && IsValidEmail(r)).ToList();

        if (recipients.Count == 0)
        {
            logger.LogDebug("📧 [SendNextStepNotifications] No recipients found for next step notifications.");
            return;
        }

        logger.LogInformation("📧 [SendNextStepNotifications] Sending next step notifications to {RecipientCount} recipients for step {StepCode}",
            recipients.Count, nextStep.StepCode);

        var baseUrl = configuration.GetValue<string>("FrontendSettings:BaseUrl") 
            ?? configuration.GetValue<string>("FrontendSettings__BaseUrl")
            ?? "https://app.qimerp.com";
        
        var reviewUrl = $"{baseUrl.TrimEnd('/')}/workflow/entity/{@event.EntityType}/{@event.EntityId}/review";

        var entityDisplayName = GetEntityDisplayName(entity);
        var initiatedByName = entity.WorkflowInitiatedByName ?? FormatEmailAsName(entity.WorkflowInitiatedByEmail) ?? "System";
        var initiatedByEmail = entity.WorkflowInitiatedByEmail ?? "system@qimerp.com";
        var initiatedAt = entity.WorkflowInitiatedAt ?? DateTime.UtcNow;

        var replacements = new Dictionary<string, string>
        {
            ["EntityType"] = @event.EntityType,
            ["EntityName"] = entityDisplayName,
            ["WorkflowCode"] = @event.WorkflowCode,
            ["StepName"] = nextStep.Name,
            ["StepDescription"] = nextStep.Description,
            ["InitiatedByName"] = initiatedByName,
            ["InitiatedByEmail"] = initiatedByEmail,
            ["InitiatedAt"] = initiatedAt.ToString("MMMM dd, yyyy 'at' HH:mm UTC"),
            ["ApproverName"] = "",
            ["WorkflowId"] = @event.WorkflowId,
            ["EntityId"] = @event.EntityId,
            ["ReviewUrl"] = reviewUrl,
            ["Year"] = DateTime.UtcNow.Year.ToString()
        };

        var emailTemplate = await templateService.RenderEmailTemplateAsync("WorkflowStarted", replacements);

        foreach (var recipient in recipients)
        {
            try
            {
                var message = new UnifiedMessageModel
                {
                    MessageType = "templated_email",
                    ToEmail = recipient,
                    Subject = $"Action Required: {entityDisplayName} - Review Request",
                    Template = emailTemplate,
                    Replacements = replacements,
                    MessageId = Guid.NewGuid().ToString(),
                    CorrelationId = @event.WorkflowId,
                    Metadata = new Dictionary<string, string>
                    {
                        ["SourceModule"] = "Workflow",
                        ["SourceEntityType"] = @event.EntityType ?? "",
                        ["SourceEntityId"] = @event.EntityId ?? "",
                        ["NotificationType"] = "WorkflowNextStep"
                    }
                };

                await publisher.PublishAsync(message, "qimerp.core.notify.prod_exchange");

                logger.LogInformation("✅ [SendNextStepNotifications] Successfully sent next step notification to {Recipient} for step {StepCode}",
                    recipient, nextStep.StepCode);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ [SendNextStepNotifications] Failed to send next step notification to {Recipient} for step {StepCode}",
                    recipient, nextStep.StepCode);
            }
        }
    }

    private async Task SendRequesterNotificationAsync(
        WorkflowApprovalRequestEvent @event,
        IWorkflowEnabled entity,
        string notificationType,
        string stepName)
    {
        if (string.IsNullOrWhiteSpace(entity.WorkflowInitiatedByEmail))
        {
            logger.LogDebug("📧 [SendRequesterNotification] WorkflowInitiatedByEmail is empty. Skipping requester notification.");
            return;
        }

        if (!IsValidEmail(entity.WorkflowInitiatedByEmail))
        {
            logger.LogWarning("⚠️ [SendRequesterNotification] WorkflowInitiatedByEmail '{Email}' is not a valid email. Skipping requester notification.",
                entity.WorkflowInitiatedByEmail);
            return;
        }

        var templateName = notificationType switch
        {
            "StepApproved" => "WorkflowStepApproved",
            "WorkflowCompleted" => "WorkflowCompletion",
            _ => "WorkflowStepApproved"
        };

        var workflowDefinition = entity.WorkflowDefinition;
        var entityDetails = ExtractEntityDetails(entity);
        var entityDisplayName = GetEntityDisplayName(entity);
        var baseUrl = configuration.GetValue<string>("FrontendSettings:BaseUrl") 
            ?? configuration.GetValue<string>("FrontendSettings__BaseUrl")
            ?? "https://app.qimerp.com";
        var reviewUrl = $"{baseUrl.TrimEnd('/')}/workflow/entity/{@event.EntityType}/{@event.EntityId}/review";
        var initiatedAt = entity.WorkflowInitiatedAt ?? DateTime.UtcNow;

        var isCompleted = notificationType == "WorkflowCompleted"
            || (!string.IsNullOrWhiteSpace(entity.CurrentWorkflowState) 
                && string.Equals(entity.CurrentWorkflowState.Trim(), AppConstant.Workflow.States.Completed, StringComparison.OrdinalIgnoreCase));

        if (isCompleted)
        {
            logger.LogDebug("✅ [SendRequesterNotification] Detected completed workflow. CurrentWorkflowState={State}, NotificationType={Type}", 
                entity.CurrentWorkflowState, notificationType);
        }

        var completedDate = isCompleted && entity.WorkflowCompletedAt.HasValue
            ? entity.WorkflowCompletedAt.Value
            : (@event.ApprovedAt != default ? @event.ApprovedAt : DateTime.UtcNow);
        var approvedDate = completedDate;

        var approvedStep = workflowDefinition?.Steps?.FirstOrDefault(s => s.Name == stepName);
        var approvedStepNumber = approvedStep?.Order ?? 0;
        var totalSteps = workflowDefinition?.Steps?.Count ?? 0;
        
        var nextStepCode = isCompleted
            ? AppConstant.Workflow.States.Completed
            : (@event.CurrentState ?? @event.NextStepCode ?? "");
        var nextStepName = "Final Review";
        if (!isCompleted && !string.IsNullOrWhiteSpace(nextStepCode) && workflowDefinition?.Steps != null)
        {
            var nextStep = workflowDefinition.Steps.FirstOrDefault(s => s.StepCode == nextStepCode);
            if (nextStep != null)
            {
                nextStepName = nextStep.Name;
            }
        }
        else if (!isCompleted && approvedStepNumber > 0 && totalSteps > approvedStepNumber && workflowDefinition?.Steps != null)
        {
            var nextStep = workflowDefinition.Steps.OrderBy(s => s.Order).Skip(approvedStepNumber).FirstOrDefault();
            if (nextStep != null)
            {
                nextStepName = nextStep.Name;
            }
        }

        var stageInfo = isCompleted
            ? (totalSteps > 1 
                ? $"All {totalSteps} Stages Completed"
                : "Workflow Completed")
            : (totalSteps > 0 && approvedStepNumber > 0
                ? $"Stage {approvedStepNumber} of {totalSteps}: Approved"
                : "Step Approved");

        var entityReference = entityDetails.TryGetValue("Code", out var code) && !string.IsNullOrWhiteSpace(code)
            ? code
            : entityDetails.TryGetValue("EmployeeNumber", out var empNo) && !string.IsNullOrWhiteSpace(empNo)
                ? empNo
                : @event.EntityId;

        var requesterDisplayName = entityDetails.TryGetValue("FullName", out var fullName) && !string.IsNullOrWhiteSpace(fullName)
            ? fullName
            : entity.WorkflowInitiatedByName ?? FormatEmailAsName(entity.WorkflowInitiatedByEmail) ?? "Requester";

        var requesterLabel = entityDetails.ContainsKey("FullName") ? "Employee Name" : "Requester Name";

        var workflowProgressHtml = workflowDefinition != null
            ? dynamicHtmlGenerator.GenerateWorkflowProgressHtml(workflowDefinition, nextStepCode, initiatedAt, isRequester: true, isCompleted: isCompleted)
            : dynamicHtmlGenerator.GenerateEmptyProgressHtml(initiatedAt);

        var actorName = isCompleted
            ? (entity.WorkflowCompletedByName ?? @event.UserName ?? "System")
            : (@event.UserName ?? "Approver");

        var replacements = new Dictionary<string, string>
        {
            ["EntityType"] = @event.EntityType,
            ["EntityName"] = entityDisplayName,
            ["StepName"] = stepName,
            ["NotificationType"] = notificationType,
            ["Comments"] = @event.Comments,
            ["ActorName"] = actorName,
            ["ApproverName"] = actorName,
            ["ActorEmail"] = isCompleted 
                ? (entity.WorkflowCompletedByEmail ?? @event.ApprovedBy)
                : (@event.ApprovedBy),
            ["WorkflowCode"] = @event.WorkflowCode,
            ["RequesterName"] = entity.WorkflowInitiatedByName ?? FormatEmailAsName(entity.WorkflowInitiatedByEmail) ?? "Requester",
            ["Date"] = completedDate.ToString("MMMM dd, yyyy"),
            ["WorkflowId"] = @event.WorkflowId,
            ["ReviewUrl"] = reviewUrl,
            ["Year"] = DateTime.UtcNow.Year.ToString(),
            ["WorkflowProgressHtml"] = workflowProgressHtml,
            ["StageInfo"] = stageInfo,
            ["NextStepName"] = nextStepName,
            ["EntityReference"] = entityReference,
            ["RequesterDisplayName"] = requesterDisplayName,
            ["RequesterLabel"] = requesterLabel,
            ["InitiatedAt"] = initiatedAt.ToString("MMMM dd, yyyy")
        };

        foreach (var detail in entityDetails)
        {
            replacements[detail.Key] = detail.Value;
        }

        try
        {
            var emailTemplate = await templateService.RenderEmailTemplateAsync(templateName, replacements);

            var message = new UnifiedMessageModel
            {
                MessageType = "templated_email",
                ToEmail = entity.WorkflowInitiatedByEmail,
                Subject = $"Workflow {notificationType}: {entityDisplayName}",
                Template = emailTemplate,
                Replacements = replacements,
                MessageId = Guid.NewGuid().ToString(),
                CorrelationId = @event.WorkflowId,
                Metadata = new Dictionary<string, string>
                {
                    ["SourceModule"] = "Workflow",
                    ["SourceEntityType"] = @event.EntityType,
                    ["SourceEntityId"] = @event.EntityId,
                    ["NotificationType"] = notificationType,
                    ["RecipientType"] = "Requester"
                }
            };

            await publisher.PublishAsync(message, "qimerp.core.notify.prod_exchange");
            logger.LogInformation("✅ [SendRequesterNotification] Successfully sent {NotificationType} notification to requester {Email}",
                notificationType, entity.WorkflowInitiatedByEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ [SendRequesterNotification] Failed to send {NotificationType} notification to requester {Email}",
                notificationType, entity.WorkflowInitiatedByEmail);
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
                return nameValue.ToString() ?? string.Empty;
            }
        }

        var titleProperty = entityType.GetProperty("Title", BindingFlags.Public | BindingFlags.Instance);
        if (titleProperty != null)
        {
            var titleValue = titleProperty.GetValue(entity);
            if (titleValue != null && !string.IsNullOrWhiteSpace(titleValue.ToString()))
            {
                return titleValue.ToString() ?? string.Empty;
            }
        }

        return entity.EntityType;
    }

    private static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        var emailRegex = new Regex(
            "^[\\w!#$%&'*+/=?`{|}~^-]+(?:\\.[\\w!#$%&'*+/=?`{|}~^-]+)*@(?:[a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        return emailRegex.IsMatch(email.Trim());
    }

    private List<string> ExtractEmailsFromApprovers(List<WorkflowApprover> approvers)
    {
        var emails = new List<string>();

        foreach (var approver in approvers)
        {
            if (!string.IsNullOrWhiteSpace(approver.Value) && IsValidEmail(approver.Value))
            {
                emails.Add(approver.Value.Trim());
                logger.LogDebug("✅ [ExtractEmailsFromApprovers] Found valid email in approver {Type} Value: {Email}", approver.Type, approver.Value);
            }

            if (!string.IsNullOrWhiteSpace(approver.ValueId) && IsValidEmail(approver.ValueId))
            {
                emails.Add(approver.ValueId.Trim());
                logger.LogDebug("✅ [ExtractEmailsFromApprovers] Found valid email in approver {Type} ValueId: {Email}", approver.Type, approver.ValueId);
            }
        }

        return emails.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private Dictionary<string, string> ExtractEntityDetails(IWorkflowEnabled entity)
    {
        var details = new Dictionary<string, string>();
        var entityType = entity.GetType();
        var entityTypeName = entityType.Name;

        try
        {
            if (entityTypeName.Equals("Employee", StringComparison.OrdinalIgnoreCase))
            {
                ExtractEmployeeDetails(entity, entityType, details);
            }
            else
            {
                ExtractGenericEntityDetails(entity, entityType, details);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "⚠️ [ExtractEntityDetails] Error extracting entity details for {EntityType}. Continuing with basic details.",
                entityTypeName);
        }

        return details;
    }

    private void ExtractEmployeeDetails(IWorkflowEnabled entity, Type entityType, Dictionary<string, string> details)
    {
        var properties = new[]
        {
            ("FullName", "FullName"),
            ("FirstName", "FirstName"),
            ("LastName", "LastName"),
            ("MiddleName", "MiddleName"),
            ("Code", "Code"),
            ("No", "EmployeeNumber"),
            ("Email", "Email"),
            ("JobTitle", "JobTitle"),
            ("JobTitleCode", "JobTitleCode"),
            ("OrganizationalUnit", "OrganizationalUnit"),
            ("OrganizationalUnitCode", "OrganizationalUnitCode"),
            ("OrganizationalUnitName", "OrganizationalUnitName"),
            ("Department", "Department"),
            ("DepartmentCode", "DepartmentCode"),
            ("DepartmentName", "DepartmentName"),
            ("Station", "Station"),
            ("StationCode", "StationCode"),
            ("StationName", "StationName"),
            ("PreferredName", "PreferredName"),
            ("Salutation", "Salutation"),
            ("Suffix", "Suffix")
        };

        foreach (var (propertyName, detailKey) in properties)
        {
            var property = entityType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property != null)
            {
                try
                {
                    var value = property.GetValue(entity);
                    if (value != null)
                    {
                        var stringValue = value switch
                        {
                            string str => str,
                            int intVal => intVal.ToString(),
                            Guid guidVal => guidVal.ToString(),
                            DateOnly dateVal => dateVal.ToString("yyyy-MM-dd"),
                            DateTime dateTimeVal => dateTimeVal.ToString("yyyy-MM-dd HH:mm:ss"),
                            _ => value.ToString() ?? ""
                        };

                        if (!string.IsNullOrWhiteSpace(stringValue))
                        {
                            details[detailKey] = stringValue;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, "⚠️ [ExtractEmployeeDetails] Error reading property {PropertyName} for Employee entity", propertyName);
                }
            }
        }
    }

    private void ExtractGenericEntityDetails(IWorkflowEnabled entity, Type entityType, Dictionary<string, string> details)
    {
        var properties = new[]
        {
            ("Name", "Name"),
            ("Title", "Title"),
            ("Code", "Code"),
            ("Email", "Email"),
            ("Description", "Description"),
            ("ReferenceCode", "ReferenceCode")
        };

        foreach (var (propertyName, detailKey) in properties)
        {
            var property = entityType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property != null)
            {
                try
                {
                    var value = property.GetValue(entity);
                    if (value != null)
                    {
                        var stringValue = value switch
                        {
                            string str => str,
                            int intVal => intVal.ToString(),
                            Guid guidVal => guidVal.ToString(),
                            DateOnly dateVal => dateVal.ToString("yyyy-MM-dd"),
                            DateTime dateTimeVal => dateTimeVal.ToString("yyyy-MM-dd HH:mm:ss"),
                            _ => value.ToString() ?? ""
                        };

                        if (!string.IsNullOrWhiteSpace(stringValue))
                        {
                            details[detailKey] = stringValue;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, "⚠️ [ExtractGenericEntityDetails] Error reading property {PropertyName} for {EntityType} entity",
                        propertyName, entityType.Name);
                }
            }
        }
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

