namespace QimErp.Shared.Common.Activities.Payroll;

/// <summary>
/// Request payload for the Payroll performance-outcome-sync Temporal worker.
/// Fired when an employee accepts a post-calibration performance outcome carrying a bonus.
/// </summary>
public class PerformanceOutcomeSyncRequest
{
    public string TenantId { get; set; } = "";
    public string TriggeredBy { get; set; } = "";

    // ── Employee ──────────────────────────────────────────────────────────────
    public Guid EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = "";
    public string EmployeeName { get; set; } = "";

    // ── Outcome ───────────────────────────────────────────────────────────────
    public Guid OutcomeId { get; set; }
    public string OutcomeCode { get; set; } = "";
    public decimal? BonusAmount { get; set; }
    public string ReviewCode { get; set; } = "";
}
