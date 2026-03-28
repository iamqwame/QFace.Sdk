namespace QimErp.Shared.Common.Activities.Payroll;

/// <summary>
/// Temporal workflow interface for payroll benefit-sync.
/// Implemented by BenefitSyncWorkflow in QimErp.Payroll.Core.WebApi.
/// Referenced by HROperations publishers so they can start the workflow
/// via IWorkflowStarter without a cross-project dependency on the Payroll WebApi.
/// Task queue: "qimerp-payroll-benefit-sync"
/// </summary>
public interface IBenefitSyncWorkflow
{
    Task RunAsync(BenefitSyncRequest request);
}
