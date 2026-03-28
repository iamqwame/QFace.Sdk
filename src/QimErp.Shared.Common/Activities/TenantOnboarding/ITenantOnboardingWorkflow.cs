using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities.TenantOnboarding;

/// <summary>
/// Shared interface for TenantOnboardingWorkflow hosted on Platform Orchestration.
/// Declared here so that other services (e.g. IAM) can dispatch the workflow
/// via IWorkflowStarter without taking a direct dependency on Platform.Orchestration.WebApi.
/// </summary>
[Workflow("TenantOnboardingWorkflow")]
public interface ITenantOnboardingWorkflow
{
    [WorkflowRun]
    Task RunAsync(TenantOnboardingStartRequest request);
}
