namespace QimErp.Shared.Common.Services.Workflow.Temporal;

/// <summary>
/// Input payload passed to the Temporal ApprovalWorkflow when it starts.
/// Serialised into Temporal's data store — keep all properties serialisation-safe (no circular refs).
/// </summary>
public class ApprovalWorkflowInput
{
    /// <summary>Matches CurrentWorkflowHistoryId set on the entity by the interceptor.</summary>
    public string WorkflowId { get; set; } = "";

    public string EntityType { get; set; } = "";
    public string EntityId { get; set; } = "";
    public string EntityName { get; set; } = "";
    public string WorkflowCode { get; set; } = "";
    public string TenantId { get; set; } = "";
    public string Module { get; set; } = "";
    public string? InitiatedBy { get; set; }
    public string? InitiatedByName { get; set; }

    /// <summary>StepCode of the first step — set by the interceptor.</summary>
    public string? CurrentState { get; set; }

    /// <summary>Build from the WorkflowEventMessage the interceptor produces.</summary>
    public static ApprovalWorkflowInput From(WorkflowEventMessage message) => new()
    {
        WorkflowId      = message.WorkflowId,
        EntityType      = message.EntityType,
        EntityId        = message.EntityId,
        EntityName      = message.EntityName,
        WorkflowCode    = message.WorkflowCode,
        TenantId        = message.TenantId,
        Module          = message.Module,
        InitiatedBy     = message.InitiatedBy,
        InitiatedByName = message.UserName,
        CurrentState    = message.CurrentState
    };
}
