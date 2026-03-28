namespace QimErp.Shared.Common.Activities.TenantOnboarding;

public interface ITenantModuleSetupWorkflow
{
    Task<TenantModuleSetupResult> RunAsync(TenantModuleSetupRequest request);
}
