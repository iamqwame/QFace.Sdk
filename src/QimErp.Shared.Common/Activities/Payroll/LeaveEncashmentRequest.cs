namespace QimErp.Shared.Common.Activities.Payroll;

/// <summary>
/// Request payload for the Payroll leave-encashment Temporal worker.
/// Leave burns balance first, then starts this workflow to create the payout line.
/// </summary>
public class LeaveEncashmentRequest
{
    public string TenantId { get; set; } = "";
    public string TriggeredBy { get; set; } = "";

    public Guid EncashmentId { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = "";
    public string EmployeeName { get; set; } = "";

    public Guid LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = "";
    public decimal NumberOfDays { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string Reason { get; set; } = "";
    public int Year { get; set; }
}
