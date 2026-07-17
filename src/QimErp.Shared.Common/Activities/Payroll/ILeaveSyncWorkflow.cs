using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities.Payroll;

/// <summary>
/// Temporal workflow interface for payroll leave-sync.
/// Implemented by LeaveSyncWorkflow in QimErp.Payroll.Core.WebApi.
/// Referenced by HROperations.Leave publishers so they can start the workflow
/// via IWorkflowStarter.
/// Task queue: "qimerp-payroll-leave-sync"
/// </summary>
[Workflow("LeaveSyncWorkflow")]
public interface ILeaveSyncWorkflow
{
    [WorkflowRun]
    Task RunAsync(LeaveSyncRequest request);
}
