namespace QimErp.Shared.Common.Events;

public class WorkflowStatusChangedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EntityType { get; set; } = "";
    public string EntityId { get; set; } = "";
    public WorkflowStatus OldStatus { get; set; }
    public WorkflowStatus NewStatus { get; set; }
    public string? Comments { get; set; }
    public string? ChangedBy { get; set; }
    public string Module { get; set; } = ""; // HR, Finance, etc.
    public Dictionary<string, object> EntityData { get; set; } = new();
    public DateTime OccurredOn { get; }
    public string TenantId { get; }
    public string? TriggeredBy { get; }
}

public class WorkflowApprovalRequiredEvent : DomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EntityType { get; set; } = "";
    public string EntityId { get; set; } = "";
    public string EntityName { get; set; } = "";
    public string WorkflowId { get; set; } = "";
    public string WorkflowCode { get; set; } = "";
    public string? RequiredApprovalLevel { get; set; }
    public string? InitiatedBy { get; set; }
    public string Module { get; set; } = "";
    public Dictionary<string, object> EntityData { get; set; } = new();
    public string? CurrentState { get; set; }
    public string? NextStepCode { get; set; }

    public WorkflowApprovalRequiredEvent()
    {
    }

    public WorkflowApprovalRequiredEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}

public class WorkflowCompletedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EntityType { get; set; } = "";
    public string EntityId { get; set; } = "";
    public string EntityName { get; set; } = "";
    public WorkflowStatus FinalStatus { get; set; }
    public string? CompletedBy { get; set; }
    public string? Comments { get; set; }
    public string Module { get; set; } = "";
    public Dictionary<string, object> EntityData { get; set; } = new();
    public DateTime OccurredOn { get; }
    public string TenantId { get; }
    public string? TriggeredBy { get; }
}

public class WorkflowTimeoutEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EntityType { get; set; } = "";
    public string EntityId { get; set; } = "";
    public string EntityName { get; set; } = "";
    public string WorkflowId { get; set; } = "";
    public int DaysOverdue { get; set; }
    public string? LastAssignedTo { get; set; }
    public string Module { get; set; } = "";
    public DateTime OccurredOn { get; }
    public string TenantId { get; }
    public string? TriggeredBy { get; }
}

public class WorkflowApprovalRequestEvent : DomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string WorkflowId { get; set; } = "";
    public string EntityType { get; set; } = "";
    public string EntityId { get; set; } = "";
    public string EntityName { get; set; } = "";
    public string WorkflowCode { get; set; } = "";
    public string? CurrentState { get; set; }
    public bool ShouldComplete { get; set; }
    public bool IsLastStep { get; set; }
    public string? NextStepCode { get; set; }
    public string? PreviousStep { get; set; }
    public string Comments { get; set; } = "";
    public string ApprovedBy { get; set; } = "";
    public DateTime ApprovedAt { get; set; }
    public string Module { get; set; } = "";
    public Dictionary<string, object> EntityData { get; set; } = new();

    public WorkflowApprovalRequestEvent()
    {
    }

    public WorkflowApprovalRequestEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}

public class WorkflowApprovalProcessedEvent : DomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string WorkflowId { get; set; } = "";
    public string EntityType { get; set; } = "";
    public string EntityId { get; set; } = "";
    public WorkflowStatus OldStatus { get; set; }
    public WorkflowStatus NewStatus { get; set; }
    public string? CurrentState { get; set; }
    public bool ShouldComplete { get; set; }
    public string? NextStepCode { get; set; }
    public string Comments { get; set; } = "";
    public string ApprovedBy { get; set; } = "";
    public DateTime ApprovedAt { get; set; }
    public string Module { get; set; } = "";
    public Dictionary<string, object> EntityData { get; set; } = new();

    public WorkflowApprovalProcessedEvent()
    {
    }
    
}

public class WorkflowRejectionRequestEvent : DomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string WorkflowId { get; set; } = "";
    public string EntityType { get; set; } = "";
    public string EntityId { get; set; } = "";
    public string EntityName { get; set; } = "";
    public string WorkflowCode { get; set; } = "";
    public string? CurrentState { get; set; }
    public string? PreviousStep { get; set; }
    public string RejectionReason { get; set; } = "";
    public string RejectedBy { get; set; } = "";
    public DateTime RejectedAt { get; set; }
    public bool ReturnToOriginator { get; set; } = true;
    public string Module { get; set; } = "";
    public Dictionary<string, object> EntityData { get; set; } = new();

    public WorkflowRejectionRequestEvent()
    {
    }

    public WorkflowRejectionRequestEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }
}
