namespace QimErp.Shared.Common.Events;

/// <summary>
/// Survey events for cross-module communication.
/// These events are consumed by other modules (e.g., Employee Engagement) without direct module references.
/// </summary>
public class SurveyCreatedEvent : DomainEvent
{
    public Guid SurveyId { get; set; }
    public string SurveyCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SurveyType { get; set; } = string.Empty; // "Engagement", "Satisfaction", etc.
    public string Status { get; set; } = string.Empty; // "Draft", "Published", "Active", etc.
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int ResponseCount { get; set; }
    public decimal CompletionRate { get; set; }

    public SurveyCreatedEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}

public class SurveyUpdatedEvent : DomainEvent
{
    public Guid SurveyId { get; set; }
    public string SurveyCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SurveyType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int ResponseCount { get; set; }
    public decimal CompletionRate { get; set; }

    public SurveyUpdatedEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}

public class SurveyResponseSubmittedEvent : DomainEvent
{
    public Guid SurveyResponseId { get; set; }
    public Guid SurveyId { get; set; }
    public string SurveyCode { get; set; } = string.Empty;
    public Guid? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? EmployeeCode { get; set; }
    public DateTime SubmittedAt { get; set; }
    public bool IsCompleted { get; set; }

    public SurveyResponseSubmittedEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}

