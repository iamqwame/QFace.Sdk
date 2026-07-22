namespace QimErp.Shared.Common.Services.Workflow;

/// <summary>
/// Ensures HR approval workflows only run when the tenant has the Workflow module installed.
/// </summary>
public interface IWorkflowModuleApprovalGate
{
    Task<bool> IsApprovalModuleEnabledAsync(string? tenantId, CancellationToken cancellationToken = default);
}
