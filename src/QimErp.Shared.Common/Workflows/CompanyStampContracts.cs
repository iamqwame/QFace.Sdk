namespace QimErp.Shared.Common.Workflows;

/// <summary>
/// Contract for the "StampCompanyOnExistingRows" activity. Every module registers an activity with
/// this name on its tenant-setup task queue (via <c>CompanyStampActivities&lt;TContext&gt;</c>),
/// stamping every non-ITenantWideEntity row of the tenant onto CompanyId. Until a module does,
/// enabling multi-company for a tenant fails and the flag stays off — the safe outcome, since
/// "" is in every allowed-company array and a second company would otherwise read the first
/// company's entire history.
/// </summary>
public sealed class CompanyStampRequest
{
    public required string TenantId { get; init; }
    public required string CompanyId { get; init; }
    public string TriggeredBy { get; init; } = string.Empty;
}

public sealed class CompanyStampResult
{
    public bool Success { get; set; }
    public int RowsStamped { get; set; }
    public string? ErrorMessage { get; set; }
}
