using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Runtime.CompilerServices;
using System.Transactions;
using QimErp.Shared.Common.Events;
using QimErp.Shared.Common.Services;
using QimErp.Shared.Common.Services.Auth;
using QimErp.Shared.Common.Services.Workflow;
using QimErp.Shared.Common.Workflow;

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

            // Bulk-seed fast path: skip workflow processing and event capture.
            // Audit metadata still applied so seeded rows have CreatedBy/At, but no events fire.
            if (BulkSeedScope.IsSuppressed)
            {
                AddAuditMetadata(context);
                ClearPendingDomainEvents(context);
                logger.LogDebug("BulkSeedScope active — skipped workflow processing and event capture for {Context}",
                    context.GetType().Name);
                return await base.SavingChangesAsync(eventData, result, cancellationToken);
            }

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
            // Bulk-seed fast path: no captured events to publish.
            if (BulkSeedScope.IsSuppressed)
            {
                _contextEvents.Remove(context);
                return await base.SavedChangesAsync(eventData, result, cancellationToken);
            }

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

            if (BulkSeedScope.IsSuppressed)
            {
                AddAuditMetadata(context);
                ClearPendingDomainEvents(context);
                logger.LogDebug("BulkSeedScope active — skipped workflow processing and event capture for {Context} (sync)",
                    context.GetType().Name);
                return base.SavingChanges(eventData, result);
            }

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
            if (BulkSeedScope.IsSuppressed)
            {
                _contextEvents.Remove(context);
                return base.SavedChanges(eventData, result);
            }

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



    private async Task PublishWorkflowEventsAsync(List<IDomainEvent> events, CancellationToken cancellationToken)
    {
        if (events.Count == 0) return;

        var failedEvents = new List<(IDomainEvent Event, Exception Exception)>();

        foreach (var domainEvent in events)
        {
            try
            {
                if (publisher != null)
                {
                    await publisher.Publish(domainEvent, cancellationToken);
                    logger.LogDebug("Successfully published {EventType} using IPublisher", domainEvent.GetType().Name);
                }
                else
                {
                    logger.LogWarning("No IPublisher registered. Event {EventType} will not be dispatched in-process.",
                        domainEvent.GetType().Name);
                    failedEvents.Add((domainEvent, new InvalidOperationException("No IPublisher registered")));
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
            .Where(e => e.State is EntityState.Added or EntityState.Modified && e.Entity.IsWorkflowEnabled)
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
                PublishedWorkflowDefinition? publishedDefinition = null;
                var definitionProvider = serviceProvider.GetService<IWorkflowDefinitionProvider>();

                if (definitionProvider != null)
                {
                    if (!string.IsNullOrWhiteSpace(workflowCode))
                    {
                        publishedDefinition = await definitionProvider.GetPublishedDefinitionAsync(
                            resolvedTenantId, workflowCode, entity.EntityType, cancellationToken);
                    }
                    else
                    {
                        publishedDefinition = await definitionProvider.GetPublishedDefinitionByEntityTypeAsync(
                            resolvedTenantId, entity.EntityType, cancellationToken);
                    }

                    if (publishedDefinition != null && string.IsNullOrWhiteSpace(workflowCode))
                    {
                        workflowCode = publishedDefinition.WorkflowCode;
                        SetWorkflowCodeOnEntity(entity, workflowCode);
                        logger.LogDebug("Auto-detected WorkflowCode={WorkflowCode} from central provider for EntityType={EntityType}",
                            workflowCode, entity.EntityType);
                    }
                }

                string? currentState = null;
                string? nextStepCode = null;
                if (publishedDefinition?.Definition?.Steps != null &&
                    publishedDefinition.Definition.Steps.Any())
                {
                    var stepsOrdered = publishedDefinition.Definition.Steps.OrderBy(s => s.Order).ToList();
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

                var bridge = serviceProvider.GetService<IWorkflowTriggerBridge>();
                if (bridge == null)
                {
                    logger.LogWarning("⚠️ [CaptureWorkflowEvents] IWorkflowTriggerBridge is not registered. Workflow approval-required event will not be triggered for {EntityType} {EntityId}.",
                        entity.EntityType, GetEntityId(entity));
                }
                else
                {
                    try
                    {
                        var handled = await bridge.TryTriggerTemporalWorkflowAsync(workflowMessage, cancellationToken);
                        if (handled)
                            logger.LogInformation("✅ [CaptureWorkflowEvents] Temporal workflow triggered for {EntityType} {EntityId}", entity.EntityType, GetEntityId(entity));
                        else
                            logger.LogWarning("⚠️ [CaptureWorkflowEvents] WorkflowTriggerBridge returned false for {EntityType} {EntityId}. No workflow was started.",
                                entity.EntityType, GetEntityId(entity));
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "❌ [CaptureWorkflowEvents] WorkflowTriggerBridge failed for {EntityType} {EntityId}. Error: {ErrorMessage}",
                            entity.EntityType, GetEntityId(entity), ex.Message);
                    }
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
        // so AddDomainEvent() events flow through MediatR's IPublisher in-process.
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
        var definitionProvider = serviceProvider.GetService<IWorkflowDefinitionProvider>();
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

            var tenantId = ResolveTenantIdForEntity(entity);
            var workflowInitiatedForCreate = false;
            EntityWorkflowConfig? entityWorkflowConfig = null;

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
                    var config = await configCacheService.GetEntityConfigAsync(module, entity.EntityType, tenantId);
                    if (config != null)
                    {
                        entityWorkflowConfig = config;
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

            PublishedWorkflowDefinition? publishedDefinition = null;
            WorkflowDefinition? resolvedDefinition = null;
            var definitionIsActive = false;

            if (!string.IsNullOrWhiteSpace(tenantId) && definitionProvider != null)
            {
                if (!string.IsNullOrWhiteSpace(workflowCode))
                {
                    publishedDefinition = await definitionProvider.GetPublishedDefinitionAsync(
                        tenantId, workflowCode, entity.EntityType, cancellationToken);
                }
                else
                {
                    publishedDefinition = await definitionProvider.GetPublishedDefinitionByEntityTypeAsync(
                        tenantId, entity.EntityType, cancellationToken);
                }

                if (publishedDefinition != null)
                {
                    resolvedDefinition = publishedDefinition.Definition;
                    definitionIsActive = publishedDefinition.IsActive;
                    workflowCode ??= publishedDefinition.WorkflowCode;
                    logger.LogDebug(
                        "Central workflow definition found for tenant={TenantId}, EntityType={EntityType}, WorkflowCode={WorkflowCode}, IsActive={IsActive}",
                        tenantId, entity.EntityType, publishedDefinition.WorkflowCode, publishedDefinition.IsActive);
                }
            }

            if (definitionIsActive && resolvedDefinition != null)
            {
                logger.LogInformation("Initiating workflow for {EntityType} {Operation} with WorkflowCode={WorkflowCode}. Published definition is active.",
                    entity.EntityType, operation, workflowCode);

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

                    await workflowService.InitiateWorkflowAsync(entity, operation, resolvedDefinition);
                    workflowInitiatedForCreate = true;

                    logger.LogInformation("Successfully initiated workflow for {EntityType} {Operation}. WorkflowCode={WorkflowCode}, WorkflowStatus={WorkflowStatus}, CurrentWorkflowHistoryId={WorkflowHistoryId}",
                        entity.EntityType, operation, entity.WorkflowCode, entity.WorkflowStatus, entity.CurrentWorkflowHistoryId);
                }
                else if (operation == "UPDATE")
                {
                    if (ShouldInitiateWorkflowOnUpdate(entity))
                    {
                        logger.LogDebug("Calling InitiateWorkflowAsync for {EntityType} UPDATE operation. WorkflowCode={WorkflowCode}, CurrentWorkflowStatus={CurrentStatus}",
                            entity.EntityType, workflowCode, entity.WorkflowStatus);

                        await workflowService.InitiateWorkflowAsync(entity, operation, resolvedDefinition);

                        logger.LogInformation("Successfully initiated workflow for {EntityType} {Operation}. WorkflowCode={WorkflowCode}, WorkflowStatus={WorkflowStatus}, CurrentWorkflowHistoryId={WorkflowHistoryId}",
                            entity.EntityType, operation, entity.WorkflowCode, entity.WorkflowStatus, entity.CurrentWorkflowHistoryId);
                    }
                    else
                    {
                        logger.LogDebug(
                            "Skipping workflow initiation for {EntityType} UPDATE. WorkflowStatus={Status}",
                            entity.EntityType, entity.WorkflowStatus);
                    }
                }
            }
            else if (publishedDefinition != null && !publishedDefinition.IsActive)
            {
                logger.LogWarning("Workflow definition found for {EntityType} WorkflowCode={WorkflowCode} but IsActive=false. Skipping workflow initiation.",
                    entity.EntityType, workflowCode);

                if (operation == "CREATE" && IsCreateWorkflowRequired(entityWorkflowConfig))
                {
                    throw new InvalidOperationException(
                        $"Workflow approval is required for {entity.EntityType} create (workflow code: {entityWorkflowConfig!.CreateWorkflowCode}), " +
                        "but no active published workflow definition was found. Ensure Platform workflow templates are published for this tenant.");
                }

                if (operation == "CREATE" && entry.Entity is AuditableEntity auditableEntity)
                {
                    auditableEntity.AsActive();
                    logger.LogDebug("No active workflow definition for {EntityType}. Keeping entity active on create.", entity.EntityType);
                }
            }
            else if (resolvedDefinition == null)
            {
                logger.LogDebug("Published workflow definition not found. Falling back to ShouldTriggerWorkflow check for {EntityType} {Operation}",
                    entity.EntityType, operation);

                var module = GetModuleFromConfiguration();
                if (string.IsNullOrWhiteSpace(module))
                {
                    logger.LogWarning("Module not configured. Cannot check ShouldTriggerWorkflow for {EntityType}", entity.EntityType);

                    if (operation == "CREATE" && entry.Entity is AuditableEntity auditableEntity)
                    {
                        auditableEntity.AsActive();
                        logger.LogDebug("Module not configured for workflow checks. Keeping {EntityType} active on create.", entity.EntityType);
                    }
                }
                else
                {
                    var shouldTrigger = await workflowService.ShouldTriggerWorkflow(entity, operation, module);
                    logger.LogDebug("ShouldTriggerWorkflow returned {ShouldTrigger} for {EntityType} {Operation} in module {Module}",
                        shouldTrigger, entity.EntityType, operation, module);

                    if (shouldTrigger)
                    {
                        if (string.IsNullOrWhiteSpace(workflowCode))
                        {
                            workflowCode = await configCacheService.GetWorkflowCodeAsync(module, entity.EntityType, operation, tenantId);
                            if (entityWorkflowConfig == null)
                                entityWorkflowConfig = await configCacheService.GetEntityConfigAsync(module, entity.EntityType, tenantId);
                        }

                        logger.LogInformation("Workflow should be triggered for {EntityType} {Operation}. Initiating workflow without EntityWorkflowStep. WorkflowCode={WorkflowCode}",
                            entity.EntityType, operation, workflowCode);

                        if (operation == "CREATE")
                        {
                            if (entry.Entity is AuditableEntity auditableEntity)
                            {
                                auditableEntity.AsDraft();
                                logger.LogDebug("Set entity {EntityType} as Draft", entity.EntityType);
                            }

                            if (!string.IsNullOrWhiteSpace(workflowCode))
                            {
                                SetWorkflowCodeOnEntity(entity, workflowCode);
                                logger.LogDebug("Set WorkflowCode={WorkflowCode} on entity {EntityType} (fallback path)", workflowCode, entity.EntityType);
                            }

                            logger.LogDebug("Calling InitiateWorkflowAsync (fallback path) for {EntityType} CREATE operation", entity.EntityType);

                            await workflowService.InitiateWorkflowAsync(entity, operation);
                            workflowInitiatedForCreate = true;

                            logger.LogInformation("Successfully initiated workflow (fallback path) for {EntityType} {Operation}. WorkflowCode={WorkflowCode}, WorkflowStatus={WorkflowStatus}, CurrentWorkflowHistoryId={WorkflowHistoryId}",
                                entity.EntityType, operation, entity.WorkflowCode, entity.WorkflowStatus, entity.CurrentWorkflowHistoryId);
                        }
                        else if (operation == "UPDATE" && ShouldInitiateWorkflowOnUpdate(entity))
                        {
                            if (!string.IsNullOrWhiteSpace(workflowCode))
                            {
                                SetWorkflowCodeOnEntity(entity, workflowCode);
                                logger.LogDebug("Set WorkflowCode={WorkflowCode} on entity {EntityType} (fallback path)", workflowCode, entity.EntityType);
                            }

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

                        if (operation == "CREATE" && entry.Entity is AuditableEntity auditableEntity)
                        {
                            auditableEntity.AsActive();
                            logger.LogDebug("No workflow trigger configured for {EntityType} create. Keeping entity active.", entity.EntityType);
                        }
                    }
                }
            }

            if (operation == "CREATE" && !workflowInitiatedForCreate && entry.Entity is AuditableEntity createAuditableEntity)
            {
                if (IsCreateWorkflowRequired(entityWorkflowConfig))
                {
                    throw new InvalidOperationException(
                        $"Workflow approval is required for {entity.EntityType} create (workflow code: {entityWorkflowConfig!.CreateWorkflowCode}), " +
                        "but the approval workflow could not be started. Ensure workflow configuration and published definitions exist for this tenant.");
                }

                createAuditableEntity.AsActive();
                logger.LogDebug("Create operation for {EntityType} finished without workflow initiation. Enforced active status.", entity.EntityType);
            }
        }
    }

    private static bool IsCreateWorkflowRequired(EntityWorkflowConfig? config) =>
        config?.EnableWorkflowForCreate == true
        && !string.IsNullOrWhiteSpace(config.CreateWorkflowCode);

    /// <summary>
    /// Only start an UPDATE workflow when the entity is not already in an active/completed approval cycle.
    /// Never re-initiate after approval finalization (Approved) or while steps are in flight (InProgress).
    /// </summary>
    private static bool ShouldInitiateWorkflowOnUpdate(IWorkflowEnabled entity) =>
        entity.WorkflowStatus is WorkflowStatus.NotStarted or WorkflowStatus.Rejected;

    private string ResolveTenantIdForEntity(IWorkflowEnabled entity)
    {
        var tenantId = userContextService?.GetTenantId() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(tenantId))
            return tenantId;

        if (entity is AuditableEntity auditableEntity && !string.IsNullOrWhiteSpace(auditableEntity.TenantId))
            return auditableEntity.TenantId;

        return string.Empty;
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
        WorkflowEntitySnapshotBuilder.GetDisplayName(entity)
        ?? entity.EntityType;

    protected virtual Dictionary<string, object> GetEntityData(IWorkflowEnabled entity) =>
        WorkflowEntitySnapshotBuilder.Build(entity);

    /// <summary>
    /// Drops any pending domain events on tracked AuditableEntity instances without publishing.
    /// Called on the BulkSeedScope fast path so seed-time entity construction (e.g. WithMiddleName
    /// triggering EmployeeChangedEvent) does not leave events queued for the next non-seed save.
    /// </summary>
    private void ClearPendingDomainEvents(DbContext context)
    {
        var tracked = context.ChangeTracker.Entries<AuditableEntity>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified)
            .ToList();
        foreach (var entry in tracked)
        {
            if (entry.Entity.HasDomainEvents)
                entry.Entity.ClearDomainEvents();
        }
    }
}
