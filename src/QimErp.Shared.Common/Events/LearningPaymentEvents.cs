namespace QimErp.Shared.Common.Events;

/// <summary>
/// Learning payment events for cross-module communication.
/// These events are consumed by other modules (e.g., Payroll) without direct module references.
/// </summary>
public class LearningSubscriptionCreatedEvent : DomainEvent
{
    public Guid SubscriptionId { get; set; }
    public string SubscriptionCode { get; set; } = string.Empty;
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string ProfessionalBodyName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int SubscriptionYear { get; set; }

    public LearningSubscriptionCreatedEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}

public class LearningSubscriptionFinanceApprovedEvent : DomainEvent
{
    public Guid SubscriptionId { get; set; }
    public string SubscriptionCode { get; set; } = string.Empty;
    public Guid? ApprovedBy { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime ApprovedDate { get; set; }

    public LearningSubscriptionFinanceApprovedEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}

public class LearningSubscriptionPaidEvent : DomainEvent
{
    public Guid SubscriptionId { get; set; }
    public string SubscriptionCode { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public string? PaymentReference { get; set; }

    public LearningSubscriptionPaidEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}

public class PaymentRequestCreatedEvent : DomainEvent
{
    public Guid PaymentRequestId { get; set; }
    public string PaymentRequestCode { get; set; } = string.Empty;
    public Guid EnrollmentId { get; set; }
    public Guid EmployeeId { get; set; }
    public decimal Amount { get; set; }
    public string RequestType { get; set; } = string.Empty;

    public PaymentRequestCreatedEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}

public class PaymentRequestFinanceApprovedEvent : DomainEvent
{
    public Guid PaymentRequestId { get; set; }
    public string PaymentRequestCode { get; set; } = string.Empty;
    public Guid? ApprovedBy { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime ApprovedDate { get; set; }

    public PaymentRequestFinanceApprovedEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}

public class PaymentRequestPaidEvent : DomainEvent
{
    public Guid PaymentRequestId { get; set; }
    public string PaymentRequestCode { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public string? PaymentReference { get; set; }

    public PaymentRequestPaidEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}

public class RefundRequestCreatedEvent : DomainEvent
{
    public Guid RefundRequestId { get; set; }
    public string RefundRequestCode { get; set; } = string.Empty;
    public Guid EnrollmentId { get; set; }
    public Guid EmployeeId { get; set; }
    public decimal RefundAmount { get; set; }

    public RefundRequestCreatedEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}

public class RefundRequestFinanceApprovedEvent : DomainEvent
{
    public Guid RefundRequestId { get; set; }
    public string RefundRequestCode { get; set; } = string.Empty;
    public Guid? ApprovedBy { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime ApprovedDate { get; set; }

    public RefundRequestFinanceApprovedEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}

public class RefundRequestProcessedEvent : DomainEvent
{
    public Guid RefundRequestId { get; set; }
    public string RefundRequestCode { get; set; } = string.Empty;
    public DateTime RefundDate { get; set; }
    public string? RefundReference { get; set; }

    public RefundRequestProcessedEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}

