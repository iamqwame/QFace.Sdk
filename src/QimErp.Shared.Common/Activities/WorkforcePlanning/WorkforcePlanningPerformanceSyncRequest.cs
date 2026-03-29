namespace QimErp.Shared.Common.Activities.WorkforcePlanning;

/// <summary>
/// Request DTO for syncing Performance module events into WorkforcePlanning.
/// Producers are in CoreHr.Performance, consumer is in CoreHr.WorkforcePlanning.
/// </summary>
public class WorkforcePlanningPerformanceSyncRequest
{
    public string EventType { get; set; } = string.Empty;
    public string EntityCode { get; set; } = string.Empty;

    public Guid? PlanId { get; set; }
    public string? PlanTitle { get; set; }
    public Guid? EmployeeId { get; set; }

    public string TenantId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string? TriggeredBy { get; set; }
    public string? UserName { get; set; }
}
