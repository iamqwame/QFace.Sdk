namespace QimErp.Shared.Common.Services.Workflow;

/// <summary>
/// Gates approval-workflow execution on whether the Workflow module is enabled for a
/// tenant. Implemented by <see cref="WorkflowModuleApprovalGate"/>.
/// </summary>
public interface IWorkflowModuleApprovalGate
{
    Task<bool> IsApprovalModuleEnabledAsync(string? tenantId, CancellationToken cancellationToken = default);
}

