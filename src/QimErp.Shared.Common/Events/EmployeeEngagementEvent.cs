namespace QimErp.Shared.Common.Events;

public class DisciplinaryCaseInterdictedEvent : DomainEvent
{
    public Guid CaseId { get; set; }
    public string CaseCode { get; set; } = string.Empty;
    public string CaseTitle { get; set; } = string.Empty;
    public Guid EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string? EmployeeEmail { get; set; }
    public DateTime InterdictionStartDate { get; set; }
    public DateTime? InterdictionEndDate { get; set; }
    public string? Reason { get; set; }
    public decimal? OriginalSalary { get; set; } // For Payroll to calculate half-pay

    public DisciplinaryCaseInterdictedEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}

public class DisciplinaryCaseExoneratedEvent : DomainEvent
{
    public Guid CaseId { get; set; }
    public string CaseCode { get; set; } = string.Empty;
    public string CaseTitle { get; set; } = string.Empty;
    public Guid EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string? EmployeeEmail { get; set; }
    public DateTime ExonerationDate { get; set; }
    public string? Reason { get; set; }

    public DisciplinaryCaseExoneratedEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}

public class DisciplinaryCaseBonusWithheldEvent : DomainEvent
{
    public Guid CaseId { get; set; }
    public string CaseCode { get; set; } = string.Empty;
    public string CaseTitle { get; set; } = string.Empty;
    public Guid EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string? EmployeeEmail { get; set; }
    public DateTime ActionDate { get; set; }
    public decimal? BonusAmount { get; set; } // If known, otherwise Payroll will calculate
    public string? Reason { get; set; }

    public DisciplinaryCaseBonusWithheldEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}

public class DisciplinaryCaseBonusReleasedEvent : DomainEvent
{
    public Guid CaseId { get; set; }
    public string CaseCode { get; set; } = string.Empty;
    public string CaseTitle { get; set; } = string.Empty;
    public Guid EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string? EmployeeEmail { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string? Reason { get; set; }

    public DisciplinaryCaseBonusReleasedEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}
