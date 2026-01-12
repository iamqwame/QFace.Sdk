namespace QimErp.Shared.Common.Events;

/// <summary>
/// Leave events for cross-module communication.
/// These events are consumed by other modules (e.g., Payroll) without direct module references.
/// </summary>
public class LeaveRequestApprovedEvent : DomainEvent
{
    public Guid LeaveRequestId { get; set; }
    public string LeaveRequestCode { get; set; } = string.Empty;
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public Guid LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int NumberOfDays { get; set; }
    public bool IsUnpaid { get; set; }
    public DateTime ApprovedAt { get; set; }

    public LeaveRequestApprovedEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}

public class LeaveRequestRejectedEvent : DomainEvent
{
    public Guid LeaveRequestId { get; set; }
    public string LeaveRequestCode { get; set; } = string.Empty;
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public Guid LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int NumberOfDays { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime RejectedAt { get; set; }

    public LeaveRequestRejectedEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}

public class TravelPermissionCreatedEvent : DomainEvent
{
    public Guid PermissionId { get; set; }
    public string PermissionCode { get; set; } = string.Empty;
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public Guid LeaveRequestId { get; set; }
    public DateTime TravelStartDate { get; set; }
    public DateTime TravelEndDate { get; set; }
    public string Destination { get; set; } = string.Empty;

    public TravelPermissionCreatedEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}

public class TravelPermissionApprovedEvent : DomainEvent
{
    public Guid PermissionId { get; set; }
    public string PermissionCode { get; set; } = string.Empty;
    public Guid? ApprovedBy { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime ApprovedDate { get; set; }

    public TravelPermissionApprovedEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}

public class TravelPermissionRejectedEvent : DomainEvent
{
    public Guid PermissionId { get; set; }
    public string PermissionCode { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }

    public TravelPermissionRejectedEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}

