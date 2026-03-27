namespace QimErp.Shared.Common.Activities;

/// <summary>
/// Temporal workflow interface for employee sync fan-out.
/// Implemented by EmployeeSyncWorkflow in QimErp.CoreHr.Employee.WebApi.
/// Referenced here so EmployeeManagementService (in Shared) can start the workflow
/// without a cross-project dependency on the WebApi.
/// </summary>
public interface IEmployeeSyncWorkflow
{
    Task RunAsync(EmployeeSyncRequest request);
}
