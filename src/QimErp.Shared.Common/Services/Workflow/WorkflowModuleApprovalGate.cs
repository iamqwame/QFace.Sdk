using QimErp.Shared.Common.Services.TenantSetup;
using QimErp.Shared.Common.TenantSetup;

namespace QimErp.Shared.Common.Services.Workflow;

public sealed class WorkflowModuleApprovalGate(
    ITenantModuleAccessService moduleAccess,
    ILogger<WorkflowModuleApprovalGate> logger) : IWorkflowModuleApprovalGate
{
    public async Task<bool> IsApprovalModuleEnabledAsync(
        string? tenantId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            logger.LogDebug(
                "[WorkflowModuleApprovalGate] Empty tenant — approval workflows disabled");
            return false;
        }

        var enabled = await moduleAccess.IsModuleEnabledAsync(
            tenantId, ModuleKeys.Workflow, cancellationToken);

        if (!enabled)
        {
            logger.LogDebug(
                "[WorkflowModuleApprovalGate] Module {ModuleKey} not installed for tenant {TenantId} — skipping approval workflow",
                ModuleKeys.Workflow, tenantId);
        }

        return enabled;
    }
}
