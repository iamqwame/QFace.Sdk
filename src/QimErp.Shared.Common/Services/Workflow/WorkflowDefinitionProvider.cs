using QimErp.Shared.Common.Services.Cache;

namespace QimErp.Shared.Common.Services.Workflow;

/// <summary>
/// Read-only workflow definition provider. Redis is the runtime source of truth —
/// a cache miss means no published workflow exists for that tenant/entity.
/// </summary>
public class WorkflowDefinitionProvider(
    IDistributedCacheService cache,
    ILogger<WorkflowDefinitionProvider> logger) : IWorkflowDefinitionProvider
{
    public async Task<PublishedWorkflowDefinition?> GetPublishedDefinitionAsync(
        string tenantId,
        string workflowCode,
        string entityType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            logger.LogWarning("Cannot load workflow definition — TenantId is empty for WorkflowCode={WorkflowCode}",
                workflowCode);
            return null;
        }

        var cacheKey = WorkflowDefinitionCacheKeys.PublishedDefinition(tenantId, workflowCode, entityType);

        try
        {
            var cached = await cache.GetAsync<PublishedWorkflowDefinition>(cacheKey);
            if (cached != null)
            {
                logger.LogDebug("Cache hit for workflow definition tenant={TenantId}, code={WorkflowCode}",
                    tenantId, workflowCode);
                return cached;
            }

            logger.LogDebug(
                "No published workflow definition in cache for tenant={TenantId}, code={WorkflowCode}, entity={EntityType}",
                tenantId, workflowCode, entityType);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error reading workflow definition from cache for tenant={TenantId}, code={WorkflowCode}",
                tenantId, workflowCode);
            return null;
        }
    }

    public async Task<PublishedWorkflowDefinition?> GetPublishedDefinitionByEntityTypeAsync(
        string tenantId,
        string entityType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            logger.LogWarning("Cannot load workflow definition — TenantId is empty for EntityType={EntityType}",
                entityType);
            return null;
        }

        var cacheKey = WorkflowDefinitionCacheKeys.PublishedDefinitionByEntityType(tenantId, entityType);

        try
        {
            var cached = await cache.GetAsync<PublishedWorkflowDefinition>(cacheKey);
            if (cached != null)
            {
                logger.LogDebug("Cache hit for workflow definition by entity tenant={TenantId}, entity={EntityType}",
                    tenantId, entityType);
                return cached;
            }

            logger.LogDebug(
                "No published workflow definition in cache for tenant={TenantId}, entity={EntityType}",
                tenantId, entityType);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error reading workflow definition from cache for tenant={TenantId}, entity={EntityType}",
                tenantId, entityType);
            return null;
        }
    }
}
