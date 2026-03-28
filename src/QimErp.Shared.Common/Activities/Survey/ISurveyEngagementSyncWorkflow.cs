namespace QimErp.Shared.Common.Activities.Survey;

/// <summary>
/// Temporal workflow interface for syncing a survey record to the EmployeeEngagement module.
/// Implemented in QimErp.HrOperations.EmployeeEngagement.WebApi (SurveyEngagementSyncWorkflow).
/// Callers are in QimErp.HrOperations.Surveys.WebApi (CreateSurvey, UpdateSurvey handlers).
/// Task queue: qimerp-engagement-survey-sync
/// </summary>
public interface ISurveyEngagementSyncWorkflow
{
    Task RunAsync(SurveyEngagementSyncRequest request);
}
