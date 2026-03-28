namespace QimErp.Shared.Common.Activities.Engagement;

/// <summary>
/// Temporal workflow interface for syncing engagement events (Risk, HealthIssue, DisciplinaryCase)
/// to CoreHR EmployeeQuery records.
/// Implemented in QimErp.CoreHr.Employee.WebApi (EngagementSyncWorkflow).
/// Callers are in QimErp.HrOperations.EmployeeEngagement.WebApi.
/// </summary>
public interface IEngagementSyncWorkflow
{
    Task ProcessRiskAsync(EngagementQuerySyncRequest request);
    Task ProcessHealthIssueAsync(EngagementQuerySyncRequest request);
    Task ProcessDisciplinaryCaseAsync(EngagementQuerySyncRequest request);
}
