namespace QimErp.Shared.Common.Workflow.Entities;

/// <summary>
/// Entity that stores workflow steps locally in each module's database
/// This allows modules to have their own copy of workflow definitions
/// and work independently without querying the central workflow module
/// </summary>
public class EntityWorkflowStep : GuidAuditableEntity
{
    /// <summary>
    /// Unique workflow code identifier (e.g., "job-requisition-approval")
    /// </summary>
    public string WorkflowCode { get; set; } = string.Empty;

    /// <summary>
    /// Entity type this workflow applies to (e.g., "JobRequisition")
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Category of the workflow (e.g., "Recruitment", "Employee", "Purchase")
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Whether the workflow is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Workflow definition containing all steps, approvers, and configuration
    /// </summary>
    public WorkflowDefinition WorkflowDefinition { get; set; } = new();

    /// <summary>
    /// Version of the workflow for tracking updates
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Reference to the original workflow template ID
    /// </summary>
    public Guid? TemplateId { get; set; }
}

