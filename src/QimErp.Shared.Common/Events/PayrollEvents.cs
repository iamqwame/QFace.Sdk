namespace QimErp.Shared.Common.Events;

/// <summary>
/// Payroll events for cross-module communication.
/// These events are consumed by other modules (e.g., Notifications, Accounting) without direct module references.
/// </summary>
public class PayrollRunCompletedEvent : DomainEvent
{
    public Guid PayrollRunId { get; set; }
    public string PayrollCode { get; set; } = string.Empty;
    public DateTime PayPeriodStart { get; set; }
    public DateTime PayPeriodEnd { get; set; }
    public DateTime PayDate { get; set; }
    public int TotalEmployees { get; set; }
    public decimal TotalGrossPay { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalNetPay { get; set; }
    public DateTime CompletedAt { get; set; }

    public PayrollRunCompletedEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}

public class PayslipGeneratedEvent : DomainEvent
{
    public Guid PayslipId { get; set; }
    public string PayslipNumber { get; set; } = string.Empty;
    public Guid PayrollItemId { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public DateTime PayPeriodStart { get; set; }
    public DateTime PayPeriodEnd { get; set; }
    public decimal GrossPay { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetPay { get; set; }
    public DateTime GeneratedAt { get; set; }

    public PayslipGeneratedEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}

