namespace QimErp.Shared.Common.Activities.Survey;

/// <summary>
/// Request DTO for syncing a survey record from Surveys module to EmployeeEngagement module.
/// Passed by Surveys.WebApi to EmployeeEngagement.WebApi via Temporal.
/// </summary>
public class SurveyEngagementSyncRequest
{
    public Guid SurveyId           { get; set; }
    public string? SurveyCode      { get; set; }
    public string Title            { get; set; } = "";
    public string? Description     { get; set; }
    public string SurveyType       { get; set; } = "";
    public string Status           { get; set; } = "";
    public DateTime? StartDate     { get; set; }
    public DateTime? EndDate       { get; set; }
    public int ResponseCount       { get; set; }
    public decimal CompletionRate  { get; set; }
    public string TenantId         { get; set; } = "";
    public string TriggeredBy      { get; set; } = "";
}
