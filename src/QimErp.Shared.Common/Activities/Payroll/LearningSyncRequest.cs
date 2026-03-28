namespace QimErp.Shared.Common.Activities.Payroll;

/// <summary>
/// Request payload for the Payroll learning-sync Temporal worker.
/// Covers: EnrollmentCompleted, CertificateIssued.
/// </summary>
public class LearningSyncRequest
{
    /// <summary>
    /// Discriminator: EnrollmentCompleted / CertificateIssued
    /// </summary>
    public string Action { get; set; } = "";

    public string TenantId { get; set; } = "";
    public string TriggeredBy { get; set; } = "";

    // ── Employee ──────────────────────────────────────────────────────────────
    public Guid EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = "";
    public string EmployeeName { get; set; } = "";

    // ── Enrollment completion fields ──────────────────────────────────────────
    public Guid EnrollmentId { get; set; }
    public decimal? Score { get; set; }
    public string? Grade { get; set; }
    public DateTime CompletedAt { get; set; }

    // ── Certificate fields ────────────────────────────────────────────────────
    public Guid? CertificateId { get; set; }
    public string? CourseTitle { get; set; }
    public DateTime IssuedAt { get; set; }
}
