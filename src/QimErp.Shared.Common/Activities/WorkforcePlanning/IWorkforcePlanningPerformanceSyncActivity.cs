using Temporalio.Activities;

namespace QimErp.Shared.Common.Activities.WorkforcePlanning;

/// <summary>
/// Activity contract implemented in CoreHr.WorkforcePlanning for Performance-to-WorkforcePlanning sync.
/// Task queue: qimerp-workforceplanning-performance-sync
/// </summary>
public interface IWorkforcePlanningPerformanceSyncActivity
{
    [Activity]
    Task SyncAsync(WorkforcePlanningPerformanceSyncRequest request);
}
