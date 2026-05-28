using QimErp.Shared.Common.Services.Cache;

namespace QimErp.Shared.Common.Services.Workflow;

public class WorkflowDefinitionCacheWriter(
    IDistributedCacheService cache,
    ILogger<WorkflowDefinitionCacheWriter> logger) : IWorkflowDefinitionCacheWriter
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(1440);

    public async Task WriteAsync(PublishedWorkflowDefinition definition, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(definition.TenantId))
        {
            logger.LogWarning("Skipping workflow definition cache write — TenantId is empty for WorkflowCode={WorkflowCode}",
                definition.WorkflowCode);
            return;
        }

        var definitionKey = WorkflowDefinitionCacheKeys.PublishedDefinition(
            definition.TenantId, definition.WorkflowCode, definition.EntityType);
        await cache.SetAsync(definitionKey, definition, CacheTtl);

        if (!string.IsNullOrWhiteSpace(definition.EntityType))
        {
            var byEntityKey = WorkflowDefinitionCacheKeys.PublishedDefinitionByEntityType(
                definition.TenantId, definition.EntityType);
            await cache.SetAsync(byEntityKey, definition, CacheTtl);
        }

        logger.LogDebug(
            "Cached published workflow definition for tenant={TenantId}, code={WorkflowCode}, entity={EntityType}",
            definition.TenantId, definition.WorkflowCode, definition.EntityType);
    }

    public async Task RemoveAsync(
        string tenantId,
        string workflowCode,
        string entityType,
        CancellationToken cancellationToken = default)
    {
        await cache.RemoveAsync(WorkflowDefinitionCacheKeys.PublishedDefinition(tenantId, workflowCode, entityType));

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            await cache.RemoveAsync(WorkflowDefinitionCacheKeys.PublishedDefinitionByEntityType(tenantId, entityType));
        }

        logger.LogDebug(
            "Removed cached workflow definition for tenant={TenantId}, code={WorkflowCode}, entity={EntityType}",
            tenantId, workflowCode, entityType);
    }
}
