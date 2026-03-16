namespace QimErp.Shared.Common.Services.Workflow.Temporal;

/// <summary>
/// Result returned by IWorkflowPlatformActivity.InitializeWorkflowRecordAsync.
/// Contains the WorkflowDefinition (so the workflow loop can iterate its steps)
/// and the platform DB record Id (used by subsequent platform activity calls).
/// </summary>
public class WorkflowInitResult
{
    /// <summary>Platform DB primary key of the newly created Workflow record.</summary>
    public Guid RecordId { get; set; }

    /// <summary>WorkflowDefinition loaded from the Platform Workflow template store.</summary>
    public WorkflowDefinition Definition { get; set; } = new();
}
