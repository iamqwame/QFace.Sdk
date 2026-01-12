namespace QimErp.Shared.Common.Events;

/// <summary>
/// Learning events for cross-module communication.
/// These events are consumed by other modules (e.g., Payroll) without direct module references.
/// </summary>
public class LearningEnrollmentCompletedEvent : DomainEvent
{
    public Guid EnrollmentId { get; set; }
    public string EnrollmentCode { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public decimal? Score { get; set; }
    public string? Grade { get; set; }
    public Guid? CertificateId { get; set; }
    public DateTime CompletedAt { get; set; }

    public LearningEnrollmentCompletedEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}

public class LearningCertificateIssuedEvent : DomainEvent
{
    public Guid CertificateId { get; set; }
    public string CertificateCode { get; set; } = string.Empty;
    public string CertificateNumber { get; set; } = string.Empty;
    public Guid EnrollmentId { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string VerificationCode { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }

    public LearningCertificateIssuedEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}

