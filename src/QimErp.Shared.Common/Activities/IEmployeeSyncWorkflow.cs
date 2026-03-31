using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities;

/// <summary>
/// Temporal workflow interface for employee sync fan-out.
/// Implemented by EmployeeSyncWorkflow in QimErp.CoreHr.Employee.WebApi.
/// Referenced here so EmployeeManagementService (in Shared) can start the workflow
/// without a cross-project dependency on the WebApi.
/// </summary>
[Workflow("EmployeeSyncWorkflow")]
public interface IEmployeeSyncWorkflow
{
    [WorkflowRun]
    Task RunAsync(EmployeeSyncRequest request);
}
