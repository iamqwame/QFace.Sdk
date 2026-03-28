namespace QimErp.Shared.Common.Activities.Payroll;

/// <summary>
/// Temporal workflow interface for payroll disciplinary-case-sync.
/// Implemented by DisciplinaryCaseSyncWorkflow in QimErp.Payroll.Core.WebApi.
/// Referenced by HROperations.EmployeeEngagement publishers so they can start
/// the workflow via IWorkflowStarter.
/// Task queue: "qimerp-payroll-disciplinary-sync"
/// </summary>
public interface IDisciplinaryCaseSyncWorkflow
{
    Task RunAsync(DisciplinaryCaseSyncRequest request);
}
