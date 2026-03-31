using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities.TenantOnboarding;

[Workflow("TenantSetupWorkflow")]
public interface ITenantModuleSetupWorkflow
{
    [WorkflowRun]
    Task<TenantModuleSetupResult> RunAsync(TenantModuleSetupRequest request);
}
