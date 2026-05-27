using QimErp.Shared.Common.Workflow;

namespace QimErp.Shared.Common.Services.Workflow;

/// <summary>
/// Write-through cache for published workflow definitions (Platform publish path).
/// </summary>
public interface IWorkflowDefinitionCacheWriter
{
    Task WriteAsync(PublishedWorkflowDefinition definition, CancellationToken cancellationToken = default);

    Task RemoveAsync(
        string tenantId,
        string workflowCode,
        string entityType,
        CancellationToken cancellationToken = default);
}
