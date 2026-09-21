using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities.Leave;

/// <summary>
/// Temporal workflow interface for Leave employee performance-rating snapshot sync.
/// Implemented by EmployeePerformanceRatingSyncWorkflow in QimErp.HrOperations.Leave.WebApi.
/// Task queue: "qimerp-leave-performance-rating-sync"
/// </summary>
[Workflow("EmployeePerformanceRatingSyncWorkflow")]
public interface IEmployeePerformanceRatingSyncWorkflow
{
    [WorkflowRun]
    Task RunAsync(EmployeePerformanceRatingSyncRequest request);
}
