namespace QimErp.Shared.Common.Activities.WorkforcePlanning;

/// <summary>
/// Request DTO for syncing Talent module events into WorkforcePlanning.
/// Producers are in CoreHr.Talent, consumer is in CoreHr.WorkforcePlanning.
/// </summary>
public class WorkforcePlanningTalentSyncRequest
{
    public string EventType { get; set; } = string.Empty;
    public string EntityCode { get; set; } = string.Empty;

    public Guid? ReviewId { get; set; }
    public Guid? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public bool? IsHighPotential { get; set; }
    public decimal? OverallRating { get; set; }

    public string TenantId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string? TriggeredBy { get; set; }
    public string? UserName { get; set; }
}
