namespace QimErp.Shared.Common.Events;

/// <summary>
/// Benefit events for cross-module communication.
/// These events are consumed by other modules (e.g., Payroll) without direct module references.
/// </summary>
public class BenefitEnrollmentCreatedEvent : DomainEvent
{
    public Guid EnrollmentId { get; set; }
    public string EnrollmentCode { get; set; } = string.Empty;
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public Guid BenefitPlanId { get; set; }
    public string BenefitPlanName { get; set; } = string.Empty;
    public string SelectedTier { get; set; } = string.Empty; // Serialized enum value
    public decimal MonthlyPremium { get; set; }
    public DateTime EffectiveDate { get; set; }

    public BenefitEnrollmentCreatedEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}

public class BenefitEnrollmentUpdatedEvent : DomainEvent
{
    public Guid EnrollmentId { get; set; }
    public string EnrollmentCode { get; set; } = string.Empty;
    public Guid EmployeeId { get; set; }
    public string SelectedTier { get; set; } = string.Empty; // Serialized enum value
    public decimal MonthlyPremium { get; set; }
    public string Status { get; set; } = string.Empty; // Serialized enum value

    public BenefitEnrollmentUpdatedEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}

public class BenefitEnrollmentTerminatedEvent : DomainEvent
{
    public Guid EnrollmentId { get; set; }
    public string EnrollmentCode { get; set; } = string.Empty;
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public Guid BenefitPlanId { get; set; }
    public DateTime TerminationDate { get; set; }

    public BenefitEnrollmentTerminatedEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}

public class BenefitLoanCreatedEvent : DomainEvent
{
    public Guid LoanId { get; set; }
    public string LoanCode { get; set; } = string.Empty;
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string LoanType { get; set; } = string.Empty;
    public decimal PrincipalAmount { get; set; }
    public string Status { get; set; } = string.Empty; // Serialized enum value

    public BenefitLoanCreatedEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}

public class BenefitLoanRepaymentEvent : DomainEvent
{
    public Guid LoanId { get; set; }
    public string LoanCode { get; set; } = string.Empty;
    public Guid EmployeeId { get; set; }
    public decimal PaymentAmount { get; set; }
    public decimal OutstandingBalance { get; set; }
    public DateTime PaymentDate { get; set; }

    public BenefitLoanRepaymentEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}

public class BenefitPlanActivatedEvent : DomainEvent
{
    public Guid PlanId { get; set; }
    public string PlanCode { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public Guid BenefitTypeId { get; set; }
    public DateTime EffectiveDate { get; set; }

    public BenefitPlanActivatedEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}

public class AccommodationAllocatedEvent : DomainEvent
{
    public Guid AccommodationId { get; set; }
    public string AccommodationCode { get; set; } = string.Empty;
    public Guid EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string? EmployeeEmail { get; set; }
    public Guid HouseCategoryId { get; set; }
    public string HouseCategoryName { get; set; } = string.Empty;
    public decimal MonthlyRent { get; set; }
    public DateTime AllocationDate { get; set; }
    public DateTime? TenancyAgreementStartDate { get; set; }
    public DateTime? TenancyAgreementEndDate { get; set; }

    public AccommodationAllocatedEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}

public class AccommodationVacatedEvent : DomainEvent
{
    public Guid AccommodationId { get; set; }
    public string AccommodationCode { get; set; } = string.Empty;
    public Guid EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string? EmployeeEmail { get; set; }
    public DateTime VacateDate { get; set; }
    public string? Reason { get; set; }

    public AccommodationVacatedEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}

