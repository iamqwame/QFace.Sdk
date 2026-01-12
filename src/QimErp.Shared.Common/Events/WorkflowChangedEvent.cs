namespace QimErp.Shared.Common.Events;

/// <summary>
/// Domain event published when a workflow template is created, updated, deleted, activated, or deactivated
/// This event is consumed by interested modules to update their local EntityWorkflowStep entities
/// </summary>
public class WorkflowChangedEvent : DomainEvent
{
    /// <summary>
    /// Unique workflow code identifier (e.g., "job-requisition-approval")
    /// </summary>
    public string WorkflowCode { get; set; } = string.Empty;

    /// <summary>
    /// Category of the workflow (e.g., "Recruitment", "Employee", "Purchase")
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Action that triggered this event
    /// </summary>
    public WorkflowChangeAction Action { get; set; }

    /// <summary>
    /// Workflow definition containing all steps, approvers, and configuration
    /// </summary>
    public WorkflowDefinition WorkflowDefinition { get; set; } = new();

    /// <summary>
    /// Whether the workflow is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Version of the workflow for tracking updates
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Reference to the original workflow template ID
    /// </summary>
    public Guid TemplateId { get; set; }

    /// <summary>
    /// Entity type this workflow applies to (e.g., "JobRequisition")
    /// This is derived from the workflow template or configuration
    /// </summary>
    public string? EntityType { get; set; }

    public WorkflowChangedEvent()
    {
    }

    public WorkflowChangedEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
    }

    public static WorkflowChangedEvent Create(
        string tenantId,
        string userEmail,
        string workflowCode,
        string category,
        WorkflowChangeAction action,
        WorkflowDefinition workflowDefinition,
        bool isActive,
        int version,
        Guid templateId,
        string? entityType = null,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new WorkflowChangedEvent(tenantId, userEmail, triggeredBy, userName)
        {
            WorkflowCode = workflowCode,
            Category = category,
            Action = action,
            WorkflowDefinition = workflowDefinition,
            IsActive = isActive,
            Version = version,
            TemplateId = templateId,
            EntityType = entityType
        };
    }
}

/// <summary>
/// Enum representing the type of change that occurred to a workflow
/// </summary>
public enum WorkflowChangeAction
{
    Created,
    Updated,
    Deleted,
    Activated,
    Deactivated
}

