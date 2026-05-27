using QimErp.Shared.Common.Workflow;

namespace QimErp.Shared.Common.Services.Workflow;

/// <summary>
/// Single read path for published workflow definitions (Redis cache → authoritative store).
/// </summary>
public interface IWorkflowDefinitionProvider
{
    Task<PublishedWorkflowDefinition?> GetPublishedDefinitionAsync(
        string tenantId,
        string workflowCode,
        string entityType,
        CancellationToken cancellationToken = default);

    Task<PublishedWorkflowDefinition?> GetPublishedDefinitionByEntityTypeAsync(
        string tenantId,
        string entityType,
        CancellationToken cancellationToken = default);
}
