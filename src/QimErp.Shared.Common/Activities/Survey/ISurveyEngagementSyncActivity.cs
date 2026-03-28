using Temporalio.Activities;

namespace QimErp.Shared.Common.Activities.Survey;

/// <summary>
/// Activity interface for syncing survey data from Surveys module into the EmployeeEngagement local database.
/// Implemented in QimErp.HrOperations.EmployeeEngagement.WebApi (SurveyEngagementSyncActivity).
/// Task queue: qimerp-engagement-survey-sync
/// </summary>
public interface ISurveyEngagementSyncActivity
{
    [Activity] Task SyncSurveyAsync(SurveyEngagementSyncRequest request);
}
