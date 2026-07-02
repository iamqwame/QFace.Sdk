using Temporalio.Activities;

namespace QimErp.Shared.Common.Activities.Engagement;

/// <summary>
/// Activity interface for creating CoreHR EmployeeQuery records when engagement events occur
/// (Risk created, Health Issue created, Disciplinary Case created).
/// Implemented in QimErp.CoreHr.People.WebApi.
/// Task queue: qimerp-corehr-engagement-sync
/// </summary>
public interface IEngagementSyncActivity
{
    [Activity] Task CreateRiskEmployeeQueryAsync(EngagementQuerySyncRequest request);
    [Activity] Task CreateHealthIssueEmployeeQueryAsync(EngagementQuerySyncRequest request);
    [Activity] Task CreateDisciplinaryCaseEmployeeQueryAsync(EngagementQuerySyncRequest request);
}
