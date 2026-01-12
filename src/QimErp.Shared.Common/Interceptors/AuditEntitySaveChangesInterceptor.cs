using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Runtime.CompilerServices;
using System.Transactions;
using QFace.Sdk.RabbitMq.Services;
using QimErp.Shared.Common.Services.Workflow;

namespace QimErp.Shared.Common.Interceptors;

public class AuditEntitySaveChangesInterceptor(
    ICurrentUserService userContextService,
    ILogger<AuditEntitySaveChangesInterceptor> logger,
    IServiceProvider serviceProvider,
    IConfiguration? configuration = null,
    IPublisher? publisher = null)
    : SaveChangesInterceptor
{
    private readonly ConditionalWeakTable<DbContext, List<IDomainEvent>> _contextEvents = [];
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        DbContext? context = eventData.Context;
        if (context == null)
        {
            logger.LogWarning("DbContext is null in SavingChangesAsync");
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        try
        {
            logger.LogDebug("Starting pre-save processing for context: {Context}", context.GetType().Name);

            // Set TenantId on entities early, before workflow processing
            SetTenantIdOnEntities(context);

            var workflowEvents = new List<IDomainEvent>();
            await ProcessWorkflowEntitiesAsync(workflowEvents, cancellationToken, context);
            await CaptureWorkflowEventsAsync(workflowEvents, context, cancellationToken);
            _contextEvents.AddOrUpdate(context, workflowEvents);
            AddAuditMetadata(context);

            logger.LogDebug("Successfully completed pre-save processing for context: {Context}", context.GetType().Name);
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred during pre-save processing for context: {Context}", context.GetType().Name);
            _contextEvents.Remove(context);
            throw;
        }
    }


    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null)
        {
            logger.LogWarning("DbContext is null in SavedChangesAsync");
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        try
        {
            if (result > 0 && _contextEvents.TryGetValue(context, out var workflowEvents))
            {
                logger.LogDebug("Publishing {EventCount} workflow events after successful save ({RowsAffected} rows affected)", 
                    workflowEvents.Count, result);
                await PublishWorkflowEventsWithTransactionAsync(workflowEvents, context, cancellationToken);
                _contextEvents.Remove(context);
            }
            else if (result == 0)
            {
                logger.LogDebug("No changes saved (result=0), skipping event publication");
                _contextEvents.Remove(context);
            }

            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred during post-save event processing for context: {Context}", context.GetType().Name);
            _contextEvents.Remove(context);
            throw;
        }
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        var context = eventData.Context;
        if (context == null)
        {
            logger.LogWarning("DbContext is null in SavingChanges");
            return base.SavingChanges(eventData, result);
        }

        try
        {
            logger.LogDebug("Starting pre-save processing for context: {Context} (sync)", context.GetType().Name);

            var workflowEvents = new List<IDomainEvent>();
            ProcessWorkflowEntitiesAsync(workflowEvents, CancellationToken.None, context).GetAwaiter().GetResult();
            CaptureWorkflowEventsAsync(workflowEvents, context, CancellationToken.None).GetAwaiter().GetResult();
            _contextEvents.AddOrUpdate(context, workflowEvents);
            AddAuditMetadata(context);

            logger.LogDebug("Successfully completed pre-save processing for context: {Context} (sync)", context.GetType().Name);
            return base.SavingChanges(eventData, result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred during pre-save processing for context: {Context} (sync)", context.GetType().Name);
            _contextEvents.Remove(context);
            throw;
        }
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        var context = eventData.Context;
        if (context == null)
        {
            logger.LogWarning("DbContext is null in SavedChanges");
            return base.SavedChanges(eventData, result);
        }

        try
        {
            if (result > 0 && _contextEvents.TryGetValue(context, out var workflowEvents))
            {
                logger.LogDebug("Publishing {EventCount} workflow events after successful save (sync)", workflowEvents.Count);
                PublishWorkflowEventsWithTransactionAsync(workflowEvents, context, CancellationToken.None).GetAwaiter().GetResult();
                _contextEvents.Remove(context);
            }
            else if (result == 0)
            {
                logger.LogDebug("No changes saved (result=0), skipping event publication");
                _contextEvents.Remove(context);
            }

            return base.SavedChanges(eventData, result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred during post-save event processing for context: {Context} (sync)", context.GetType().Name);
            _contextEvents.Remove(context);
            throw;
        }
    }

    
    private async Task PublishWorkflowEventsWithTransactionAsync(
        List<IDomainEvent> events,
        DbContext context,
        CancellationToken cancellationToken)
    {
        if (publisher == null || events.Count == 0) return;

        logger.LogDebug("Publishing {EventCount} workflow events with transaction", events.Count);

        using var transactionScope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = TimeSpan.FromMinutes(5)
            },
            TransactionScopeAsyncFlowOption.Enabled);

        try
        {
            await PublishWorkflowEventsAsync(events, cancellationToken);
            transactionScope.Complete();
            logger.LogDebug("Successfully published {EventCount} workflow events with transaction", events.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish workflow events with transaction, rolling back");
            throw;
        }
    }

    private async Task PublishWorkflowEventsWithDbTransactionAsync(
        List<IDomainEvent> events,
        DbContext context,
        CancellationToken cancellationToken)
    {
        if (publisher == null || events.Count == 0) return;

        logger.LogDebug("Publishing {EventCount} workflow events with DB transaction", events.Count);

        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await PublishWorkflowEventsAsync(events, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            logger.LogDebug("Successfully published {EventCount} workflow events with DB transaction", events.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish workflow events with DB transaction, rolling back");
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task PublishWorkflowEventsAsync(List<IDomainEvent> events, CancellationToken cancellationToken)
    {
        if (events.Count == 0) return;

        var rabbitMqPublisher = serviceProvider.GetService<IRabbitMqPublisher>();
        var failedEvents = new List<(IDomainEvent Event, Exception Exception)>();

        foreach (var domainEvent in events)
        {
            try
            {
                var exchangeName = GetExchangeNameForEvent(domainEvent);
                
                if (!string.IsNullOrEmpty(exchangeName) && rabbitMqPublisher != null)
                {
                    // Use IRabbitMqPublisher with explicit exchange name for workflow events
                    await rabbitMqPublisher.PublishAsync(domainEvent, exchangeName);
                    logger.LogDebug("Successfully published {EventType} to exchange {ExchangeName}", 
                        domainEvent.GetType().Name, exchangeName);
                }
                else if (publisher != null)
                {
                    // Fallback to IPublisher for non-workflow events or if IRabbitMqPublisher is not available
                    await publisher.Publish(domainEvent, cancellationToken);
                    logger.LogDebug("Successfully published {EventType} using IPublisher", domainEvent.GetType().Name);
                }
                else
                {
                    logger.LogWarning("No publisher available for event {EventType}. Event will not be published.", 
                        domainEvent.GetType().Name);
                    failedEvents.Add((domainEvent, new InvalidOperationException("No publisher available")));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to publish workflow event {EventType}", domainEvent.GetType().Name);
                failedEvents.Add((domainEvent, ex));
            }
        }

        if (failedEvents.Count > 0)
        {
            var failedEventTypes = string.Join(", ", failedEvents.Select(f => f.Event.GetType().Name));
            logger.LogError("Failed to publish {FailedCount} out of {TotalCount} events: {EventTypes}", 
                failedEvents.Count, events.Count, failedEventTypes);

            if (failedEvents.Any(f => IsCriticalEvent(f.Event)))
            {
                throw new InvalidOperationException($"Critical workflow events failed to publish: {failedEventTypes}");
            }

            await StoreFailedEventsAsync(failedEvents, cancellationToken);
        }
    }

    private static string? GetExchangeNameForEvent(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            WorkflowChangedEvent => "qimerp.workflow.workflow_changed.prod_exchange",
            WorkflowStatusChangedEvent => "qimerp.workflow.workflow_status_changed.prod_exchange",
            WorkflowCompletedEvent => "qimerp.workflow.workflow_completed.prod_exchange",
            _ => null // Return null to use IPublisher fallback for other events
        };
    }

    private bool IsCriticalEvent(IDomainEvent domainEvent)
    {
        return domainEvent is WorkflowStatusChangedEvent or WorkflowApprovalRequiredEvent or WorkflowChangedEvent;
    }

    private async Task StoreFailedEventsAsync(
        List<(IDomainEvent Event, Exception Exception)> failedEvents,
        CancellationToken cancellationToken)
    {
        foreach (var (eventItem, exception) in failedEvents)
        {
            logger.LogWarning("Storing failed event {EventType} for later retry. Error: {Error}",
                eventItem.GetType().Name, exception.Message);
        }
    }
    private void SetTenantIdOnEntities(DbContext context)
    {
        var tenantId = userContextService?.GetTenantId() ?? string.Empty;
        
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            logger.LogDebug("TenantId is empty in userContextService. Skipping TenantId assignment on entities.");
            return;
        }

        var entries = context.ChangeTracker.Entries<AuditableEntity>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified);

        foreach (var entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Entity.TenantId))
            {
                entry.Entity.TenantId = tenantId;
                logger.LogDebug("Set TenantId={TenantId} on {EntityType} {EntityId} before workflow processing",
                    tenantId, entry.Entity.GetType().Name, entry.Entity.GetType().GetProperty("Id")?.GetValue(entry.Entity));
            }
        }
    }

    private void AddAuditMetadata(DbContext context)
    {
        var entries = context.ChangeTracker.Entries<AuditableEntity>();

        foreach (var entry in entries)
        {
            var userId = userContextService.GetUserId();
            var tenantId = userContextService.GetTenantId();
            var email = userContextService.GetUserEmail();
            var name = userContextService.GetUserName();
            var timestamp = DateTime.UtcNow;

            try
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        logger.LogDebug("Processing added entity: {EntityType}, Id: {EntityId}",
                            entry.Entity.GetType().Name, entry.Entity.TenantId);
                        entry.Entity
                            .OnCreate(userId, email, name)
                            .AddAuditMetadata(userId, email, name, timestamp);

                        if (string.IsNullOrWhiteSpace(entry.Entity.TenantId))
                        {
                            entry.Entity.TenantId = tenantId;
                        }

                        logger.LogDebug("Added metadata to entity: {EntityType}, TenantId: {EntityId}",
                            entry.Entity.GetType().Name, entry.Entity.TenantId);
                        break;

                    case EntityState.Modified:
                        logger.LogDebug("Processing modified entity: {EntityType}, TenantId: {EntityId}",
                            entry.Entity.GetType().Name, entry.Entity.TenantId);
                        entry.Entity
                            .AddAuditMetadata(userId, email, name, timestamp);

                        if (entry.Entity.DataStatus != entry.Entity.PreviousDataStatus)
                        {
                            entry.Entity.OnDataStatusChange(entry.Entity.DataStatus ?? DataState.Active);
                        }

                        logger.LogDebug("Modified metadata for entity: {EntityType}, TenantId: {EntityId}",
                            entry.Entity.GetType().Name, entry.Entity.TenantId);
                        break;

                    case EntityState.Detached:
                    case EntityState.Unchanged:
                    case EntityState.Deleted:
                    default:
                        logger.LogDebug(
                            "Skipping entity state: {EntityState}, EntityType: {EntityType}, TenantId: {EntityId}",
                            entry.State, entry.Entity.GetType().Name, entry.Entity.TenantId);
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while processing entity: {EntityType}, TenantId: {EntityId}",
                    entry.Entity.GetType().Name, entry.Entity.TenantId);
                throw;
            }
        }
    }


    private async Task CaptureWorkflowEventsAsync(List<IDomainEvent> events, DbContext? context, CancellationToken cancellationToken)
    {
        if (context == null) return;
        
        var currentUser = userContextService?.GetUserId();
        if (string.IsNullOrWhiteSpace(currentUser))
            currentUser = "system";
        var tenantId = userContextService?.GetTenantId() ?? string.Empty;
        string moduleName = GetModuleFromConfiguration();

        List<EntityEntry<IWorkflowEnabled>> statusChanges = context.ChangeTracker.Entries<IWorkflowEnabled>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified)
            .ToList();

        foreach (EntityEntry<IWorkflowEnabled> entry in statusChanges)
        {
            IWorkflowEnabled entity = entry.Entity;
            
            WorkflowStatus oldStatus;
            var originalValue = entry.Property(nameof(IWorkflowEnabled.WorkflowStatus)).OriginalValue;
            
            if (entry.State == EntityState.Added)
            {
                oldStatus = WorkflowStatus.NotStarted;
                logger.LogDebug("[CaptureWorkflowEvents] Entity {EntityType} {EntityId} is Added. Assuming oldStatus=NotStarted (default for new entities)",
                    entity.EntityType, GetEntityId(entity));
            }
            else if (originalValue == null)
            {
                oldStatus = WorkflowStatus.NotStarted;
                logger.LogWarning("[CaptureWorkflowEvents] OriginalValue is null for {EntityType} {EntityId} in state {EntityState}. Assuming oldStatus=NotStarted",
                    entity.EntityType, GetEntityId(entity), entry.State);
            }
            else
            {
                oldStatus = (WorkflowStatus)originalValue;
            }
            
            WorkflowStatus newStatus = entity.WorkflowStatus;
            
            logger.LogDebug("[CaptureWorkflowEvents] Checking status change for {EntityType} {EntityId}. EntityState={EntityState}, OldStatus={OldStatus}, NewStatus={NewStatus}",
                entity.EntityType, GetEntityId(entity), entry.State, oldStatus, newStatus);

            WorkflowStatusChangedEvent statusChangeEvent = new()
            {
                EntityType = entity.EntityType,
                EntityId = GetEntityId(entity),
                OldStatus = oldStatus,
                NewStatus = newStatus,
                Comments = entity.WorkflowComments,
                ChangedBy = currentUser,
                Module = moduleName,
                EntityData = GetEntityData(entity)
            };

            events.Add(statusChangeEvent);

            if (oldStatus == WorkflowStatus.NotStarted && newStatus == WorkflowStatus.InProgress)
            {
                logger.LogInformation("[CaptureWorkflowEvents] ✅ Status change detected: {EntityType} {EntityId} changed from NotStarted to InProgress. Will publish workflow event.",
                    entity.EntityType, GetEntityId(entity));
                
                // Resolve TenantId - try userContextService first, then entity as fallback
                var resolvedTenantId = tenantId;
                var tenantIdSource = "userContextService";
                
                if (string.IsNullOrWhiteSpace(resolvedTenantId))
                {
                    // Try to get TenantId from the entity itself (if it's an AuditableEntity)
                    if (entity is AuditableEntity auditableEntity && !string.IsNullOrWhiteSpace(auditableEntity.TenantId))
                    {
                        resolvedTenantId = auditableEntity.TenantId;
                        tenantIdSource = "entity";
                        logger.LogDebug("[CaptureWorkflowEvents] TenantId from userContextService is empty. Using TenantId from entity: {TenantId}",
                            resolvedTenantId);
                    }
                    else
                    {
                        logger.LogWarning("⚠️ [CaptureWorkflowEvents] TenantId is empty from both userContextService and entity for {EntityType} {EntityId}",
                            entity.EntityType, GetEntityId(entity));
                    }
                }
                
                // Validate TenantId before proceeding
                if (string.IsNullOrWhiteSpace(resolvedTenantId))
                {
                    logger.LogError("❌ [CaptureWorkflowEvents] Cannot create WorkflowEventMessage - TenantId is empty for {EntityType} {EntityId}. Workflow event will not be published.",
                        entity.EntityType, GetEntityId(entity));
                    continue; // Skip this entity and continue with the next one
                }
                
                logger.LogDebug("[CaptureWorkflowEvents] Using TenantId={TenantId} (source: {TenantIdSource}) for {EntityType} {EntityId}",
                    resolvedTenantId, tenantIdSource, entity.EntityType, GetEntityId(entity));
                
                var workflowCode = GetWorkflowCodeFromEntity(entity);
                EntityWorkflowStep? entityWorkflowStep = null;
                
                if (context is IWorkflowAwareContext)
                {
                    entityWorkflowStep = await GetEntityWorkflowStepByEntityTypeAsync(context, entity.EntityType);
                    if (entityWorkflowStep != null)
                    {
                        if (string.IsNullOrWhiteSpace(workflowCode))
                        {
                            workflowCode = entityWorkflowStep.WorkflowCode;
                            SetWorkflowCodeOnEntity(entity, workflowCode);
                            logger.LogDebug("Auto-detected WorkflowCode={WorkflowCode} for EntityType={EntityType}", 
                                workflowCode, entity.EntityType);
                        }
                    }
                }
                
                string? currentState = null;
                string? nextStepCode = null;
                if (entityWorkflowStep?.WorkflowDefinition?.Steps != null && 
                    entityWorkflowStep.WorkflowDefinition.Steps.Any())
                {
                    var stepsOrdered = entityWorkflowStep.WorkflowDefinition.Steps.OrderBy(s => s.Order).ToList();
                    if (stepsOrdered.Count > 0)
                    {
                        currentState = stepsOrdered[0].StepCode;
                        logger.LogDebug("Determined CurrentState={CurrentState} from first step for {EntityType}", 
                            currentState, entity.EntityType);
                    }
                    if (stepsOrdered.Count > 1)
                    {
                        nextStepCode = stepsOrdered[1].StepCode;
                        logger.LogDebug("Determined NextStepCode={NextStepCode} from second step for {EntityType}", 
                            nextStepCode, entity.EntityType);
                    }
                }
                
                var workflowHistoryId = entity.CurrentWorkflowHistoryId?.ToString() ?? "";
                
                if (string.IsNullOrWhiteSpace(workflowHistoryId))
                {
                    logger.LogWarning("⚠️ [CaptureWorkflowEvents] CurrentWorkflowHistoryId is not set for {EntityType} {EntityId}. Workflow history may not be recorded correctly. WorkflowCode={WorkflowCode}",
                        entity.EntityType, GetEntityId(entity), workflowCode);
                    
                    var fallbackWorkflowHistoryId = Guid.NewGuid();
                    entity.CurrentWorkflowHistoryId = fallbackWorkflowHistoryId;
                    workflowHistoryId = fallbackWorkflowHistoryId.ToString();
                    
                    logger.LogInformation("Generated fallback WorkflowHistoryId={WorkflowHistoryId} for {EntityType} {EntityId}",
                        workflowHistoryId, entity.EntityType, GetEntityId(entity));
                }
                else
                {
                    logger.LogDebug("CurrentWorkflowHistoryId={WorkflowHistoryId} is set for {EntityType} {EntityId}",
                        workflowHistoryId, entity.EntityType, GetEntityId(entity));
                }
                
                var actorService = serviceProvider.GetService<IActorService>();
                if (actorService != null)
                {
                    logger.LogDebug("[CaptureWorkflowEvents] IActorService found. Creating WorkflowEventMessage for {EntityType} {EntityId}",
                        entity.EntityType, GetEntityId(entity));
                    
                    var workflowMessage = new WorkflowEventMessage
                    {
                        EntityType = entity.EntityType,
                        EntityId = GetEntityId(entity),
                        EntityName = GetEntityDisplayName(entity),
                        WorkflowCode = workflowCode ?? string.Empty,
                        WorkflowId = workflowHistoryId,
                        RequiredApprovalLevel = null,
                        InitiatedBy = userContextService?.GetUserEmail() ?? currentUser,
                        Module = moduleName,
                        EntityData = GetEntityData(entity),
                        TenantId = resolvedTenantId,
                        TriggeredBy = currentUser,
                        UserName = userContextService?.GetUserName(),
                        CurrentState = currentState,
                        NextStepCode = nextStepCode
                    };
                    
                    logger.LogDebug("[CaptureWorkflowEvents] Created WorkflowEventMessage for {EntityType} {EntityId}. WorkflowId={WorkflowId}, WorkflowCode={WorkflowCode}, TenantId={TenantId} (source: {TenantIdSource})",
                        entity.EntityType, GetEntityId(entity), workflowHistoryId, workflowCode, resolvedTenantId, tenantIdSource);
                    
                    try
                    {
                        logger.LogDebug("[CaptureWorkflowEvents] Calling actorService.Tell<WorkflowEventPublisherActor> for {EntityType} {EntityId}",
                            entity.EntityType, GetEntityId(entity));
                        
                        actorService.Tell<WorkflowEventPublisherActor>(workflowMessage);
                        
                        logger.LogInformation("✅ [CaptureWorkflowEvents] Successfully told WorkflowEventPublisherActor to publish workflow approval required event for {EntityType} {EntityId} with WorkflowId={WorkflowId}",
                            entity.EntityType, GetEntityId(entity), workflowHistoryId);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "❌ [CaptureWorkflowEvents] Failed to tell WorkflowEventPublisherActor for {EntityType} {EntityId}. Event will not be published. Error: {ErrorMessage}",
                            entity.EntityType, GetEntityId(entity), ex.Message);
                    }
                }
                else
                {
                    logger.LogWarning("⚠️ [CaptureWorkflowEvents] IActorService is NULL. Workflow approval required event will not be published for {EntityType} {EntityId}",
                        entity.EntityType, GetEntityId(entity));
                }
            }
            else
            {
                logger.LogDebug("[CaptureWorkflowEvents] Status change condition not met for {EntityType} {EntityId}. OldStatus={OldStatus}, NewStatus={NewStatus}. Expected: NotStarted -> InProgress",
                    entity.EntityType, GetEntityId(entity), oldStatus, newStatus);
            }

            if (newStatus is WorkflowStatus.Approved or WorkflowStatus.Rejected)
            {
                events.Add(new WorkflowCompletedEvent
                {
                    EntityType = entity.EntityType,
                    EntityId = GetEntityId(entity),
                    EntityName = GetEntityDisplayName(entity),
                    FinalStatus = newStatus,
                    CompletedBy = currentUser,
                    Comments = entity.WorkflowComments,
                    Module = moduleName,
                    EntityData = GetEntityData(entity)
                });
            }
        }

        // Collect domain events from all AuditableEntity instances (e.g., WorkflowTemplate, WorkflowConfiguration)
        // This ensures events added via AddDomainEvent() are published to RabbitMQ
        var auditableEntities = context.ChangeTracker.Entries<AuditableEntity>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified)
            .ToList();

        foreach (var entry in auditableEntities)
        {
            var domainEvents = entry.Entity.PullDomainEvents();
            if (domainEvents.Any())
            {
                events.AddRange(domainEvents);
                logger.LogDebug("Collected {EventCount} domain event(s) from {EntityType}", 
                    domainEvents.Count(), entry.Entity.GetType().Name);
            }
        }
    }



    private async Task ProcessWorkflowEntitiesAsync(List<IDomainEvent> events, CancellationToken cancellationToken,
        DbContext? context)
    {
        if (context == null) return;

        var configCacheService = serviceProvider.GetService<IWorkflowConfigCacheService>();
        var workflowService = serviceProvider.GetService<IWorkflowService>();
        if (configCacheService == null || workflowService == null) return;

        var currentUser = userContextService.GetUserId();
        var userRoles = userContextService.GetUserRoles();

        var workflowEntities = context.ChangeTracker.Entries<IWorkflowEnabled>()
            .Where(e => e.Entity.IsWorkflowEnabled)
            .ToList();

        foreach (var entry in workflowEntities)
        {
            var entity = entry.Entity;
            var operation = entry.State switch
            {
                EntityState.Added => "CREATE",
                EntityState.Modified => "UPDATE",
                EntityState.Deleted => "DELETE",
                _ => null
            };

            if (operation == null) continue;

            string? workflowCode = GetWorkflowCodeFromEntity(entity);

            if (string.IsNullOrWhiteSpace(workflowCode))
            {
                var module = GetModuleFromConfiguration();
                if (string.IsNullOrWhiteSpace(module))
                {
                    logger.LogWarning("Module not configured in appsettings. Skipping workflow configuration lookup for {EntityType}", entity.EntityType);
                }
                else
                {
                    var config = await configCacheService.GetEntityConfigAsync(module, entity.EntityType);
                    if (config != null)
                    {
                        workflowCode = operation.ToUpper() switch
                        {
                            "CREATE" => config.CreateWorkflowCode,
                            "UPDATE" => config.UpdateWorkflowCode,
                            "DELETE" => config.DeleteWorkflowCode,
                            _ => null
                        };

                        if (IsUserExcluded(config, currentUser, userRoles))
                            continue;

                        if (entry.State == EntityState.Modified)
                        {
                            var canEdit = await ValidateCanEditAsync(entity, config);
                            if (!canEdit.IsSuccess)
                            {
                                throw new InvalidOperationException(canEdit.Error.Message);
                            }
                        }

                        if (entry.State == EntityState.Deleted)
                        {
                            var canDelete = await ValidateCanDeleteAsync(entity, config);
                            if (!canDelete.IsSuccess)
                            {
                                throw new InvalidOperationException(canDelete.Error.Message);
                            }
                        }
                        
                        // Validate SignificantFieldsForUpdate if this is an UPDATE operation
                        if (entry.State == EntityState.Modified && config.SignificantFieldsForUpdate.Any())
                        {
                            ValidateSignificantFields(entity, config.SignificantFieldsForUpdate);
                        }
                    }
                }
            }

            EntityWorkflowStep? entityWorkflowStep = null;
            if (!string.IsNullOrWhiteSpace(workflowCode) && context is IWorkflowAwareContext workflowContext)
            {
                logger.LogDebug("Querying EntityWorkflowStep for EntityType={EntityType}, WorkflowCode={WorkflowCode}", 
                    entity.EntityType, workflowCode);
                
                entityWorkflowStep = await GetEntityWorkflowStepAsync(context, entity.EntityType, workflowCode);
                
                if (entityWorkflowStep == null)
                {
                    logger.LogDebug("EntityWorkflowStep not found by EntityType and WorkflowCode. Trying fallback query by WorkflowCode only. WorkflowCode={WorkflowCode}", 
                        workflowCode);
                    
                    entityWorkflowStep = await GetEntityWorkflowStepByWorkflowCodeAsync(context, workflowCode);
                    
                    if (entityWorkflowStep != null)
                    {
                        logger.LogWarning("Found EntityWorkflowStep by WorkflowCode only. EntityType in DB may be empty. WorkflowCode={WorkflowCode}, DB EntityType={DbEntityType}", 
                            workflowCode, entityWorkflowStep.EntityType);
                    }
                    else
                    {
                        logger.LogDebug("EntityWorkflowStep not found by WorkflowCode fallback query. WorkflowCode={WorkflowCode}", 
                            workflowCode);
                    }
                }
                else
                {
                    logger.LogDebug("Found EntityWorkflowStep for EntityType={EntityType}, WorkflowCode={WorkflowCode}, IsActive={IsActive}", 
                        entity.EntityType, workflowCode, entityWorkflowStep.IsActive);
                }
            }
            else if (string.IsNullOrWhiteSpace(workflowCode))
            {
                logger.LogDebug("WorkflowCode is empty for EntityType={EntityType}. Skipping EntityWorkflowStep lookup.", 
                    entity.EntityType);
            }
            else if (context is not IWorkflowAwareContext)
            {
                logger.LogDebug("DbContext {ContextType} does not implement IWorkflowAwareContext. Skipping EntityWorkflowStep lookup for EntityType={EntityType}", 
                    context.GetType().Name, entity.EntityType);
            }

            if (entityWorkflowStep != null && entityWorkflowStep.IsActive)
            {
                logger.LogInformation("Initiating workflow for {EntityType} {Operation} with WorkflowCode={WorkflowCode}. EntityWorkflowStep found and is active.", 
                    entity.EntityType, operation, workflowCode);
                
                // Ensure WorkflowCode is set on entity before calling InitiateWorkflowAsync
                if (!string.IsNullOrWhiteSpace(workflowCode))
                {
                    SetWorkflowCodeOnEntity(entity, workflowCode);
                    logger.LogDebug("Set WorkflowCode={WorkflowCode} on entity {EntityType}", workflowCode, entity.EntityType);
                }
                
                if (operation == "CREATE")
                {
                    if (entry.Entity is AuditableEntity auditableEntity)
                    {
                        auditableEntity.AsDraft();
                        logger.LogDebug("Set entity {EntityType} as Draft", entity.EntityType);
                    }

                    logger.LogDebug("Calling InitiateWorkflowAsync for {EntityType} CREATE operation. WorkflowCode={WorkflowCode}, CurrentWorkflowHistoryId={WorkflowHistoryId}", 
                        entity.EntityType, workflowCode, entity.CurrentWorkflowHistoryId);
                    
                    await workflowService.InitiateWorkflowAsync(entity, operation, entityWorkflowStep.WorkflowDefinition);
                    
                    logger.LogInformation("Successfully initiated workflow for {EntityType} {Operation}. WorkflowCode={WorkflowCode}, WorkflowStatus={WorkflowStatus}, CurrentWorkflowHistoryId={WorkflowHistoryId}", 
                        entity.EntityType, operation, entity.WorkflowCode, entity.WorkflowStatus, entity.CurrentWorkflowHistoryId);
                }
                else if (operation == "UPDATE" && entity.WorkflowStatus != WorkflowStatus.InProgress)
                {
                    logger.LogDebug("Calling InitiateWorkflowAsync for {EntityType} UPDATE operation. WorkflowCode={WorkflowCode}, CurrentWorkflowStatus={CurrentStatus}", 
                        entity.EntityType, workflowCode, entity.WorkflowStatus);
                    
                    await workflowService.InitiateWorkflowAsync(entity, operation, entityWorkflowStep.WorkflowDefinition);
                    
                    logger.LogInformation("Successfully initiated workflow for {EntityType} {Operation}. WorkflowCode={WorkflowCode}, WorkflowStatus={WorkflowStatus}, CurrentWorkflowHistoryId={WorkflowHistoryId}", 
                        entity.EntityType, operation, entity.WorkflowCode, entity.WorkflowStatus, entity.CurrentWorkflowHistoryId);
                }
                else if (operation == "UPDATE" && entity.WorkflowStatus == WorkflowStatus.InProgress)
                {
                    logger.LogDebug("Skipping workflow initiation for {EntityType} UPDATE. Entity already has WorkflowStatus=InProgress", 
                        entity.EntityType);
                }
            }
            else if (entityWorkflowStep != null && !entityWorkflowStep.IsActive)
            {
                logger.LogWarning("EntityWorkflowStep found for {EntityType} WorkflowCode={WorkflowCode} but IsActive=false. Skipping workflow initiation.", 
                    entity.EntityType, workflowCode);
            }
            else if (entityWorkflowStep == null)
            {
                logger.LogDebug("EntityWorkflowStep not found. Falling back to ShouldTriggerWorkflow check for {EntityType} {Operation}", 
                    entity.EntityType, operation);
                
                var module = GetModuleFromConfiguration();
                if (string.IsNullOrWhiteSpace(module))
                {
                    logger.LogWarning("Module not configured. Cannot check ShouldTriggerWorkflow for {EntityType}", entity.EntityType);
                }
                else
                {
                    var shouldTrigger = await workflowService.ShouldTriggerWorkflow(entity, operation, module);
                    logger.LogDebug("ShouldTriggerWorkflow returned {ShouldTrigger} for {EntityType} {Operation} in module {Module}", 
                        shouldTrigger, entity.EntityType, operation, module);
                    
                    if (shouldTrigger)
                    {
                        logger.LogInformation("Workflow should be triggered for {EntityType} {Operation}. Initiating workflow without EntityWorkflowStep.", 
                            entity.EntityType, operation);
                        
                        if (operation == "CREATE")
                        {
                            if (entry.Entity is AuditableEntity auditableEntity)
                            {
                                auditableEntity.AsDraft();
                                logger.LogDebug("Set entity {EntityType} as Draft", entity.EntityType);
                            }

                            logger.LogDebug("Calling InitiateWorkflowAsync (fallback path) for {EntityType} CREATE operation", entity.EntityType);
                            
                            await workflowService.InitiateWorkflowAsync(entity, operation);
                            
                            logger.LogInformation("Successfully initiated workflow (fallback path) for {EntityType} {Operation}. WorkflowCode={WorkflowCode}, WorkflowStatus={WorkflowStatus}, CurrentWorkflowHistoryId={WorkflowHistoryId}", 
                                entity.EntityType, operation, entity.WorkflowCode, entity.WorkflowStatus, entity.CurrentWorkflowHistoryId);
                        }
                        else if (operation == "UPDATE" && entity.WorkflowStatus != WorkflowStatus.InProgress)
                        {
                            logger.LogDebug("Calling InitiateWorkflowAsync (fallback path) for {EntityType} UPDATE operation", entity.EntityType);
                            
                            await workflowService.InitiateWorkflowAsync(entity, operation);
                            
                            logger.LogInformation("Successfully initiated workflow (fallback path) for {EntityType} {Operation}. WorkflowCode={WorkflowCode}, WorkflowStatus={WorkflowStatus}, CurrentWorkflowHistoryId={WorkflowHistoryId}", 
                                entity.EntityType, operation, entity.WorkflowCode, entity.WorkflowStatus, entity.CurrentWorkflowHistoryId);
                        }
                    }
                    else
                    {
                        logger.LogDebug("Workflow should not be triggered for {EntityType} {Operation}. Skipping workflow initiation.", 
                            entity.EntityType, operation);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets EntityWorkflowStep from the current DbContext
    /// </summary>
    private async Task<EntityWorkflowStep?> GetEntityWorkflowStepAsync(DbContext context, string entityType, string workflowCode)
    {
        try
        {
            var entityWorkflowStepsProperty = context.GetType().GetProperty("EntityWorkflowSteps");
            if (entityWorkflowStepsProperty == null)
            {
                logger.LogDebug("DbContext {ContextType} does not have EntityWorkflowSteps DbSet", context.GetType().Name);
                return null;
            }

            var dbSet = entityWorkflowStepsProperty.GetValue(context);
            if (dbSet == null)
            {
                logger.LogDebug("EntityWorkflowSteps DbSet is null in DbContext {ContextType}", context.GetType().Name);
                return null;
            }

            // Use reflection to call AsNoTracking, Where, and FirstOrDefaultAsync
            var asNoTrackingMethod = typeof(EntityFrameworkQueryableExtensions)
                .GetMethods()
                .FirstOrDefault(m => m.Name == "AsNoTracking" && m.GetParameters().Length == 1);
            
            if (asNoTrackingMethod == null) return null;
            
            var queryableType = dbSet.GetType().GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryable<>));
            
            if (queryableType == null) return null;
            
            var entityTypeParam = queryableType.GetGenericArguments()[0];
            var asNoTrackingGeneric = asNoTrackingMethod.MakeGenericMethod(entityTypeParam);
            var query = asNoTrackingGeneric.Invoke(null, new[] { dbSet });
            
            if (query == null) return null;

            var parameter = Expression.Parameter(entityTypeParam, "e");
            var workflowCodeProperty = Expression.Property(parameter, nameof(EntityWorkflowStep.WorkflowCode));
            var entityTypeProperty = Expression.Property(parameter, nameof(EntityWorkflowStep.EntityType));
            var isActiveProperty = Expression.Property(parameter, nameof(EntityWorkflowStep.IsActive));
            var dataStatusProperty = Expression.Property(parameter, nameof(EntityWorkflowStep.DataStatus));
            
            var workflowCodeEqual = Expression.Equal(workflowCodeProperty, Expression.Constant(workflowCode));
            var entityTypeEqual = Expression.Equal(entityTypeProperty, Expression.Constant(entityType));
            var isActiveTrue = Expression.Equal(isActiveProperty, Expression.Constant(true));
            var dataStatusActive = Expression.Equal(dataStatusProperty, Expression.Constant(DataState.Active, typeof(DataState?)));
            
            var and1 = Expression.AndAlso(workflowCodeEqual, entityTypeEqual);
            var and2 = Expression.AndAlso(and1, isActiveTrue);
            var and3 = Expression.AndAlso(and2, dataStatusActive);
            
            var lambdaType = typeof(Func<,>).MakeGenericType(entityTypeParam, typeof(bool));
            var lambda = Expression.Lambda(lambdaType, and3, parameter);
            
            var whereMethod = typeof(Queryable).GetMethods()
                .FirstOrDefault(m => m.Name == "Where" && m.GetParameters().Length == 2);
            var whereGenericMethod = whereMethod?.MakeGenericMethod(entityTypeParam);
            
            var filteredQuery = whereGenericMethod?.Invoke(null, new[] { query, lambda });
            if (filteredQuery == null) return null;

            // Call FirstOrDefaultAsync
            var firstOrDefaultMethod = typeof(EntityFrameworkQueryableExtensions)
                .GetMethods()
                .FirstOrDefault(m => m.Name == "FirstOrDefaultAsync" && m.GetParameters().Length == 2);
            var firstOrDefaultGeneric = firstOrDefaultMethod?.MakeGenericMethod(entityTypeParam);
            
            var cancellationToken = CancellationToken.None;
            var task = firstOrDefaultGeneric?.Invoke(null, new[] { filteredQuery, cancellationToken }) as Task<EntityWorkflowStep?>;
            if (task == null) return null;

            return await task;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error querying EntityWorkflowStep for EntityType={EntityType}, WorkflowCode={WorkflowCode}", 
                entityType, workflowCode);
            return null;
        }
    }

    /// <summary>
    /// Gets EntityWorkflowStep from the current DbContext by EntityType only (without requiring WorkflowCode)
    /// </summary>
    private async Task<EntityWorkflowStep?> GetEntityWorkflowStepByEntityTypeAsync(DbContext context, string entityType)
    {
        try
        {
            var entityWorkflowStepsProperty = context.GetType().GetProperty("EntityWorkflowSteps");
            if (entityWorkflowStepsProperty == null)
            {
                logger.LogDebug("DbContext {ContextType} does not have EntityWorkflowSteps DbSet", context.GetType().Name);
                return null;
            }

            var dbSet = entityWorkflowStepsProperty.GetValue(context);
            if (dbSet == null)
            {
                logger.LogDebug("EntityWorkflowSteps DbSet is null in DbContext {ContextType}", context.GetType().Name);
                return null;
            }

            var asNoTrackingMethod = typeof(EntityFrameworkQueryableExtensions)
                .GetMethods()
                .FirstOrDefault(m => m.Name == "AsNoTracking" && m.GetParameters().Length == 1);
            
            if (asNoTrackingMethod == null) return null;
            
            var queryableType = dbSet.GetType().GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryable<>));
            
            if (queryableType == null) return null;
            
            var entityTypeParam = queryableType.GetGenericArguments()[0];
            var asNoTrackingGeneric = asNoTrackingMethod.MakeGenericMethod(entityTypeParam);
            var query = asNoTrackingGeneric.Invoke(null, new[] { dbSet });
            
            if (query == null) return null;

            var parameter = Expression.Parameter(entityTypeParam, "e");
            var entityTypeProperty = Expression.Property(parameter, nameof(EntityWorkflowStep.EntityType));
            var dataStatusProperty = Expression.Property(parameter, nameof(EntityWorkflowStep.DataStatus));
            
            var entityTypeEqual = Expression.Equal(entityTypeProperty, Expression.Constant(entityType));
            var dataStatusActive = Expression.Equal(dataStatusProperty, Expression.Constant(DataState.Active, typeof(DataState?)));
            
            var andExpression = Expression.AndAlso(entityTypeEqual, dataStatusActive);
            
            var lambdaType = typeof(Func<,>).MakeGenericType(entityTypeParam, typeof(bool));
            var lambda = Expression.Lambda(lambdaType, andExpression, parameter);
            
            var whereMethod = typeof(Queryable).GetMethods()
                .FirstOrDefault(m => m.Name == "Where" && m.GetParameters().Length == 2);
            var whereGenericMethod = whereMethod?.MakeGenericMethod(entityTypeParam);
            
            var filteredQuery = whereGenericMethod?.Invoke(null, new[] { query, lambda });
            if (filteredQuery == null) return null;

            var firstOrDefaultMethod = typeof(EntityFrameworkQueryableExtensions)
                .GetMethods()
                .FirstOrDefault(m => m.Name == "FirstOrDefaultAsync" && m.GetParameters().Length == 2);
            var firstOrDefaultGeneric = firstOrDefaultMethod?.MakeGenericMethod(entityTypeParam);
            
            var cancellationToken = CancellationToken.None;
            var task = firstOrDefaultGeneric?.Invoke(null, new[] { filteredQuery, cancellationToken }) as Task<EntityWorkflowStep?>;
            if (task == null) return null;

            return await task;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error querying EntityWorkflowStep by EntityType={EntityType}", entityType);
            return null;
        }
    }

    /// <summary>
    /// Gets EntityWorkflowStep from the current DbContext by WorkflowCode only (fallback when EntityType is empty in DB)
    /// </summary>
    private async Task<EntityWorkflowStep?> GetEntityWorkflowStepByWorkflowCodeAsync(DbContext context, string workflowCode)
    {
        try
        {
            var entityWorkflowStepsProperty = context.GetType().GetProperty("EntityWorkflowSteps");
            if (entityWorkflowStepsProperty == null)
            {
                logger.LogDebug("DbContext {ContextType} does not have EntityWorkflowSteps DbSet", context.GetType().Name);
                return null;
            }

            var dbSet = entityWorkflowStepsProperty.GetValue(context);
            if (dbSet == null)
            {
                logger.LogDebug("EntityWorkflowSteps DbSet is null in DbContext {ContextType}", context.GetType().Name);
                return null;
            }

            var asNoTrackingMethod = typeof(EntityFrameworkQueryableExtensions)
                .GetMethods()
                .FirstOrDefault(m => m.Name == "AsNoTracking" && m.GetParameters().Length == 1);
            
            if (asNoTrackingMethod == null) return null;
            
            var queryableType = dbSet.GetType().GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryable<>));
            
            if (queryableType == null) return null;
            
            var entityTypeParam = queryableType.GetGenericArguments()[0];
            var asNoTrackingGeneric = asNoTrackingMethod.MakeGenericMethod(entityTypeParam);
            var query = asNoTrackingGeneric.Invoke(null, new[] { dbSet });
            
            if (query == null) return null;

            var parameter = Expression.Parameter(entityTypeParam, "e");
            var workflowCodeProperty = Expression.Property(parameter, nameof(EntityWorkflowStep.WorkflowCode));
            var isActiveProperty = Expression.Property(parameter, nameof(EntityWorkflowStep.IsActive));
            var dataStatusProperty = Expression.Property(parameter, nameof(EntityWorkflowStep.DataStatus));
            
            var workflowCodeEqual = Expression.Equal(workflowCodeProperty, Expression.Constant(workflowCode));
            var isActiveTrue = Expression.Equal(isActiveProperty, Expression.Constant(true));
            var dataStatusActive = Expression.Equal(dataStatusProperty, Expression.Constant(DataState.Active, typeof(DataState?)));
            
            var and1 = Expression.AndAlso(workflowCodeEqual, isActiveTrue);
            var and2 = Expression.AndAlso(and1, dataStatusActive);
            
            var lambdaType = typeof(Func<,>).MakeGenericType(entityTypeParam, typeof(bool));
            var lambda = Expression.Lambda(lambdaType, and2, parameter);
            
            var whereMethod = typeof(Queryable).GetMethods()
                .FirstOrDefault(m => m.Name == "Where" && m.GetParameters().Length == 2);
            var whereGenericMethod = whereMethod?.MakeGenericMethod(entityTypeParam);
            
            var filteredQuery = whereGenericMethod?.Invoke(null, new[] { query, lambda });
            if (filteredQuery == null) return null;

            var firstOrDefaultMethod = typeof(EntityFrameworkQueryableExtensions)
                .GetMethods()
                .FirstOrDefault(m => m.Name == "FirstOrDefaultAsync" && m.GetParameters().Length == 2);
            var firstOrDefaultGeneric = firstOrDefaultMethod?.MakeGenericMethod(entityTypeParam);
            
            var cancellationToken = CancellationToken.None;
            var task = firstOrDefaultGeneric?.Invoke(null, new[] { filteredQuery, cancellationToken }) as Task<EntityWorkflowStep?>;
            if (task == null) return null;

            return await task;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error querying EntityWorkflowStep by WorkflowCode={WorkflowCode}", workflowCode);
            return null;
        }
    }

    /// <summary>
    /// Gets workflow code from entity if it has a WorkflowCode property
    /// </summary>
    private string? GetWorkflowCodeFromEntity(IWorkflowEnabled entity)
    {
        try
        {
            var entityType = entity.GetType();
            var workflowCodeProperty = entityType.GetProperty("WorkflowCode");
            if (workflowCodeProperty != null)
            {
                return workflowCodeProperty.GetValue(entity)?.ToString();
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Sets WorkflowCode property on entity using reflection
    /// </summary>
    private void SetWorkflowCodeOnEntity(IWorkflowEnabled entity, string workflowCode)
    {
        try
        {
            var entityType = entity.GetType();
            var workflowCodeProperty = entityType.GetProperty("WorkflowCode");
            if (workflowCodeProperty != null && workflowCodeProperty.CanWrite)
            {
                workflowCodeProperty.SetValue(entity, workflowCode);
                logger.LogDebug("Auto-set WorkflowCode={WorkflowCode} on entity {EntityType}", 
                    workflowCode, entity.EntityType);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to set WorkflowCode on entity {EntityType}", entity.EntityType);
        }
    }

    private bool IsUserExcluded(EntityWorkflowConfig config, string currentUser, List<string> userRoles)
    {
        if (config.ExcludeUsers.Contains(currentUser))
            return true;

        return userRoles.Any(role => config.ExcludeRoles.Contains(role));
    }

    private async Task<Result> ValidateCanEditAsync(IWorkflowEnabled entity, EntityWorkflowConfig config)
    {
        if (!entity.CanBeEdited())
        {
            return Result.WithFailure(new Error(
                "WorkflowValidation.CannotEdit",
                $"Entity cannot be edited in current workflow status: {entity.WorkflowStatus}"));
        }

        return Result.WithSuccess();
    }

    private async Task<Result> ValidateCanDeleteAsync(IWorkflowEnabled entity, EntityWorkflowConfig config)
    {
        if (!entity.CanBeDeleted())
        {
            return Result.WithFailure(new Error(
                "WorkflowValidation.CannotDelete",
                $"Entity cannot be deleted in current workflow status: {entity.WorkflowStatus}"));
        }

        return Result.WithSuccess();
    }
    
    /// <summary>
    /// Validates that significant fields for update exist on the entity
    /// Logs warnings for missing fields but doesn't fail (graceful degradation)
    /// </summary>
    private void ValidateSignificantFields(IWorkflowEnabled entity, List<string> significantFields)
    {
        if (significantFields == null || !significantFields.Any())
            return;
        
        var entityType = entity.GetType();
        var missingFields = new List<string>();
        
        foreach (var fieldName in significantFields)
        {
            if (!ValidateFieldExists(entity, fieldName))
            {
                missingFields.Add(fieldName);
            }
        }
        
        if (missingFields.Any())
        {
            logger.LogWarning(
                "Some significant fields for update do not exist on entity {EntityType}: {MissingFields}. " +
                "These fields will be ignored during workflow evaluation.",
                entityType.Name, string.Join(", ", missingFields));
        }
    }
    
    /// <summary>
    /// Checks if a field exists on the entity using reflection
    /// </summary>
    private bool ValidateFieldExists(IWorkflowEnabled entity, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
            return false;
        
        var entityType = entity.GetType();
        var property = entityType.GetProperty(fieldName, 
            System.Reflection.BindingFlags.Public | 
            System.Reflection.BindingFlags.Instance | 
            System.Reflection.BindingFlags.IgnoreCase);
        
        return property != null;
    }

  


    /// <summary>
    /// Gets the module name from configuration (appsettings.json)
    /// Falls back to "Unknown" if not configured
    /// </summary>
    protected virtual string GetModuleFromConfiguration()
    {
        if (configuration == null)
        {
            logger.LogWarning("IConfiguration not available. Cannot determine module name.");
            return string.Empty;
        }
        
        // Try multiple configuration keys for flexibility
        var module = configuration["Workflow:Module"] 
                  ?? configuration["Workflow:ModuleCategory"]
                  ?? configuration["Workflow:ModuleName"];
        
        if (string.IsNullOrWhiteSpace(module))
        {
            logger.LogWarning("Workflow:Module not configured in appsettings.json. Workflow configurations will not be loaded.");
            return string.Empty;
        }
        
        return module;
    }
    
    protected virtual string GetModuleName() => GetModuleFromConfiguration();

    protected virtual string GetEntityId(IWorkflowEnabled entity) =>
        entity.GetType().GetProperty("Id")?.GetValue(entity)?.ToString() ?? "";

    protected virtual string GetEntityDisplayName(IWorkflowEnabled entity) =>
        entity.GetType().GetProperty("Name")?.GetValue(entity)?.ToString() ?? entity.EntityType;

    protected virtual Dictionary<string, object> GetEntityData(IWorkflowEnabled entity) => [];
}
