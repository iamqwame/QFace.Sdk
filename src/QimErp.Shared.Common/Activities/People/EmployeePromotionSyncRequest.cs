namespace QimErp.Shared.Common.Activities.People;

/// <summary>
/// Request payload for the People employee-promotion-sync Temporal worker.
/// Fired when an employee accepts a post-calibration performance outcome carrying a
/// salary, grade, or role change recommendation. Creates a Draft EmployeePromotion
/// in People — People's own approval workflow takes over from there.
/// </summary>
public class EmployeePromotionSyncRequest
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
    public decimal? NewSalaryAmount { get; set; }
    public string? NewGradeCode { get; set; }
    public string? PromotionNewRole { get; set; }
    public string ReviewCode { get; set; } = "";
}
