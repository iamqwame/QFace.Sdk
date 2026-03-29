namespace QimErp.Shared.Common.Activities.WorkforcePlanning;

/// <summary>
/// Workflow contract implemented in CoreHr.WorkforcePlanning for Performance-to-WorkforcePlanning sync.
/// </summary>
public interface IWorkforcePlanningPerformanceSyncWorkflow
{
    Task RunAsync(WorkforcePlanningPerformanceSyncRequest request);
}
