namespace QimErp.Shared.Common.Activities.Benefit;

// Pushed by the module that owns the claim when it is approved or reversed. Benefit measures
// medical headroom against these snapshots rather than reading the owning module's tables.
public class MedicalClaimConsumptionSyncRequest
{
    public string Action { get; set; } = string.Empty;
    public string? TenantId { get; set; }
    public string? TriggeredBy { get; set; }

    public Guid EmployeeId { get; set; }
    public DateOnly ClaimDate { get; set; }
    public decimal ApprovedAmount { get; set; }
    public string? CurrencyCode { get; set; }

    public string SourceModule { get; set; } = string.Empty;
    public Guid SourceClaimId { get; set; }
    public string? SourceClaimCode { get; set; }

    public string? ReversalReason { get; set; }
}

public static class MedicalClaimConsumptionSyncActions
{
    public const string ClaimApproved = "MedicalClaimApproved";
    public const string ClaimReversed = "MedicalClaimReversed";
}
