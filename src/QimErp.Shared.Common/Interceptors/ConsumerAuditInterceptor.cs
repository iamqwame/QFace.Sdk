namespace QimErp.Shared.Common.Interceptors;

/// <summary>
/// Interceptor for consumer applications to handle audit metadata
/// Simpler than AuditEntitySaveChangesInterceptor - focuses only on audit fields
/// Does not handle workflow validation or event publishing (those are API concerns)
/// </summary>
public class ConsumerAuditInterceptor(
    ILogger<ConsumerAuditInterceptor> logger) : SaveChangesInterceptor
{
    private string? _currentTenantId;
    private string? _currentTriggeredBy;

    /// <summary>
    /// Sets the tenant context for the current operation
    /// Call this at the start of each consumer method with the event's tenantId and triggeredBy
    /// </summary>
    public void SetContext(string tenantId, string? triggeredBy = null)
    {
        _currentTenantId = tenantId;
        _currentTriggeredBy = triggeredBy ?? "system";
        logger.LogDebug("Consumer context set: TenantId={TenantId}, TriggeredBy={TriggeredBy}", tenantId, _currentTriggeredBy);
    }

    /// <summary>
    /// Clears the context after the operation completes
    /// </summary>
    public void ClearContext()
    {
        _currentTenantId = null;
        _currentTriggeredBy = null;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null)
        {
            logger.LogWarning("DbContext is null in SavingChangesAsync");
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        try
        {
            AddAuditMetadata(context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred during consumer audit processing");
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
            AddAuditMetadata(context);
            return base.SavingChanges(eventData, result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred during consumer audit processing (sync)");
            throw;
        }
    }

    private void AddAuditMetadata(DbContext context)
    {
        if (string.IsNullOrEmpty(_currentTenantId))
        {
            logger.LogWarning("TenantId not set in consumer context. Call SetContext() before SaveChanges.");
            return;
        }

        var now = DateTime.UtcNow;
        var triggeredBy = _currentTriggeredBy ?? "system";

        var entries = context.ChangeTracker.Entries<AuditableEntity>();

        foreach (var entry in entries)
        {
            var entity = entry.Entity;

            // Set TenantId for new entities
            if (entry.State == EntityState.Added && string.IsNullOrEmpty(entity.TenantId))
            {
                entity.TenantId = _currentTenantId;
                logger.LogDebug("Set TenantId={TenantId} for {EntityType}", _currentTenantId, entity.GetType().Name);
            }

            // Set audit metadata
            switch (entry.State)
            {
                case EntityState.Added:
                    entity.OnCreate(triggeredBy, triggeredBy, triggeredBy)
                          .AddAuditMetadata(triggeredBy, triggeredBy, triggeredBy, now);
                    logger.LogDebug("Set audit metadata for new {EntityType}", entity.GetType().Name);
                    break;

                case EntityState.Modified:
                    entity.OnModify(triggeredBy, triggeredBy, triggeredBy);
                    logger.LogDebug("Updated audit metadata for modified {EntityType}", entity.GetType().Name);
                    break;

                case EntityState.Deleted:
                    // Soft delete by changing DataStatus instead of actually deleting
                    entry.State = EntityState.Modified;
                    entity.OnSoftRemove();
                    entity.OnModify(triggeredBy, triggeredBy, triggeredBy);
                    logger.LogDebug("Soft deleted {EntityType}", entity.GetType().Name);
                    break;
            }
        }
    }
}
