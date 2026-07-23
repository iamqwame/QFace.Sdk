namespace QimErp.Shared.Common.Activities.Payroll;

/// <summary>
/// Temporal workflow interface for payroll performance-outcome-sync.
/// Implemented by PerformanceOutcomeSyncWorkflow in QimErp.Payroll.Core.WebApi.
/// Referenced by CoreHr.Performance publishers so they can start the workflow
/// via IWorkflowStarter.
/// Task queue: "qimerp-payroll-performance-outcome-sync"
/// </summary>
public interface IPerformanceOutcomeSyncWorkflow
{
    Task RunAsync(PerformanceOutcomeSyncRequest request);
}
