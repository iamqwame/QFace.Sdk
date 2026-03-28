namespace QimErp.Shared.Common.Activities.Payroll;

/// <summary>
/// Temporal workflow interface for payroll learning-sync.
/// Implemented by LearningSyncWorkflow in QimErp.Payroll.Core.WebApi.
/// Referenced by CoreHr.Learning publishers so they can start the workflow
/// via IWorkflowStarter.
/// Task queue: "qimerp-payroll-learning-sync"
/// </summary>
public interface ILearningSyncWorkflow
{
    Task RunAsync(LearningSyncRequest request);
}
