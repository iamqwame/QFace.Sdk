using Temporalio.Activities;

namespace QimErp.Shared.Common.Activities.WorkforcePlanning;

/// <summary>
/// Activity contract implemented in CoreHr.WorkforcePlanning for Talent-to-WorkforcePlanning sync.
/// Task queue: qimerp-workforceplanning-talent-sync
/// </summary>
public interface IWorkforcePlanningTalentSyncActivity
{
    [Activity]
    Task SyncAsync(WorkforcePlanningTalentSyncRequest request);
}
