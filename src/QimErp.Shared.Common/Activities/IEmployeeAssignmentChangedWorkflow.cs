using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities;

/// <summary>
/// Temporal workflow interface for fanning out job-title / org-unit assignment changes
/// to downstream modules (Payroll, etc.).
/// Implemented by EmployeeAssignmentChangedWorkflow in QimErp.CoreHr.Employee.WebApi.
/// </summary>
[Workflow("EmployeeAssignmentChangedWorkflow")]
public interface IEmployeeAssignmentChangedWorkflow
{
    [WorkflowRun]
    Task RunAsync(EmployeeAssignmentChangedRequest request);
}
