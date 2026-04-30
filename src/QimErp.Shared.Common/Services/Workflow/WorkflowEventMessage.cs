namespace QimErp.Shared.Common.Services.Workflow;

/// <summary>
/// In-process message describing a workflow trigger. Built by the audit interceptor
/// when a workflow-enabled entity transitions and consumed by <see cref="IWorkflowTriggerBridge"/>
/// to start a Temporal workflow.
/// </summary>
public class WorkflowEventMessage
{
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string WorkflowCode { get; set; } = string.Empty;
    public string WorkflowId { get; set; } = string.Empty;
    public string? RequiredApprovalLevel { get; set; }
    public string? InitiatedBy { get; set; }
    public string Module { get; set; } = string.Empty;
    public Dictionary<string, object> EntityData { get; set; } = new();
    public string TenantId { get; set; } = string.Empty;
    public string? TriggeredBy { get; set; }
    public string? UserName { get; set; }
    public string? CurrentState { get; set; }
    public string? NextStepCode { get; set; }
}
