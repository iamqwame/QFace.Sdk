namespace QimErp.Shared.Common.Activities.Payroll;

/// <summary>
/// Request payload for the Payroll leave-sync Temporal worker.
/// Covers: LeaveApproved, LeaveRejected.
/// </summary>
public class LeaveSyncRequest
{
    /// <summary>
    /// Discriminator: Approved / Rejected
    /// </summary>
    public string Action { get; set; } = "";

    public string TenantId { get; set; } = "";
    public string TriggeredBy { get; set; } = "";

    // ── Employee ──────────────────────────────────────────────────────────────
    public Guid EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = "";
    public string EmployeeName { get; set; } = "";

    // ── Leave request fields ──────────────────────────────────────────────────
    public Guid LeaveRequestId { get; set; }
    public Guid LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal NumberOfDays { get; set; }
    public bool IsUnpaid { get; set; }
    public string? RejectionReason { get; set; }
}
