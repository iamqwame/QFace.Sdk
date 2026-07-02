using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities;

/// <summary>
/// Temporal workflow interface for bulk employee sync during imports.
/// Implemented by EmployeeBulkSyncWorkflow in QimErp.CoreHr.People.WebApi.
///
/// During a bulk import, the import processor fires one EmployeeBulkSyncWorkflow per
/// chunk of 200 employees instead of one EmployeeSyncWorkflow per employee.
/// Each module receives a single bulk activity call (AddRangeAsync) rather than
/// 200 individual round-trips — reducing downstream DB writes by ~200x per module.
/// </summary>
[Workflow("EmployeeBulkSyncWorkflow")]
public interface IEmployeeBulkSyncWorkflow
{
    [WorkflowRun]
    Task RunAsync(EmployeeBulkSyncRequest request);
}
