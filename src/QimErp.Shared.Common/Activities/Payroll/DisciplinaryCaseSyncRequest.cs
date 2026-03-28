namespace QimErp.Shared.Common.Activities.Payroll;

/// <summary>
/// Request payload for the Payroll disciplinary-case-sync Temporal worker.
/// Covers: Interdicted, Exonerated, BonusWithheld, BonusReleased.
/// </summary>
public class DisciplinaryCaseSyncRequest
{
    /// <summary>
    /// Discriminator: Interdicted / Exonerated / BonusWithheld / BonusReleased
    /// </summary>
    public string Action { get; set; } = "";

    public string TenantId { get; set; } = "";
    public string TriggeredBy { get; set; } = "";

    // ── Employee ──────────────────────────────────────────────────────────────
    public Guid EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = "";
    public string EmployeeName { get; set; } = "";

    // ── Case ──────────────────────────────────────────────────────────────────
    public string CaseCode { get; set; } = "";
    public string? Reason { get; set; }

    // ── Interdiction fields ───────────────────────────────────────────────────
    public DateTime InterdictionStartDate { get; set; }

    // ── Exoneration fields ────────────────────────────────────────────────────
    public DateTime ExonerationDate { get; set; }

    // ── Bonus withhold / release fields ──────────────────────────────────────
    public DateTime ActionDate { get; set; }
    public decimal? BonusAmount { get; set; }
    public DateTime ReleaseDate { get; set; }
}
