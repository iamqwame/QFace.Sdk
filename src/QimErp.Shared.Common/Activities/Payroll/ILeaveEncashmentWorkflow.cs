using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities.Payroll;

/// <summary>
/// Temporal workflow interface for payroll leave encashment.
/// Implemented by LeaveEncashmentWorkflow in QimErp.Payroll.Core.WebApi.
/// Task queue: "qimerp-payroll-leave-sync"
/// </summary>
[Workflow("LeaveEncashmentWorkflow")]
public interface ILeaveEncashmentWorkflow
{
    [WorkflowRun]
    Task RunAsync(LeaveEncashmentRequest request);
}
