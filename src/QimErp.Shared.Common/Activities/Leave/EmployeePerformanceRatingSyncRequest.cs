namespace QimErp.Shared.Common.Activities.Leave;

/// <summary>
/// Payload for syncing an employee's latest performance OverallRating into Leave's local Employee snapshot.
/// Fired by CoreHr.Performance; consumed by Leave's Temporal worker on qimerp-leave-performance-rating-sync.
/// </summary>
public class EmployeePerformanceRatingSyncRequest
{
    public string TenantId { get; set; } = "";
    public string CompanyId { get; set; } = "";
    public string TriggeredBy { get; set; } = "";

    public Guid EmployeeId { get; set; }
    public Guid ReviewId { get; set; }
    public string ReviewCode { get; set; } = "";
    public decimal OverallRating { get; set; }
    public DateTime RatedAtUtc { get; set; }
}
