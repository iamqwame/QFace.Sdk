namespace QimErp.Shared.Common.Activities.WorkforcePlanning;

/// <summary>
/// Workflow contract implemented in CoreHr.WorkforcePlanning for Talent-to-WorkforcePlanning sync.
/// </summary>
public interface IWorkforcePlanningTalentSyncWorkflow
{
    Task RunAsync(WorkforcePlanningTalentSyncRequest request);
}
