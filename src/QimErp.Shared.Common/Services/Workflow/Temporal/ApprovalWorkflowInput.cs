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
    /// <summary>
    /// Initiator's resolved profile picture URL. Populated by <c>ApprovalWorkflow.RunAsync</c>
    /// after a single call to <c>IApproverResolverActivity.ResolveInitiatorByEmailAsync</c>,
    /// before the workflow record is persisted, so the row carries it from the start.
    /// </summary>
    public string? InitiatedByImage { get; set; }
    /// <summary>Initiator's resolved Employee Id (Guid string). Empty for system actors.</summary>
    public string? InitiatedByEmployeeId { get; set; }

    /// <summary>StepCode of the first step — set by the interceptor.</summary>
    public string? CurrentState { get; set; }

    /// <summary>
    /// Optional context type for approver/notification resolution (module-defined, e.g. Employee).
    /// </summary>
    public string? SubjectContextType { get; set; }

    /// <summary>
    /// Optional context id for approver/notification resolution (e.g. parent employee id).
    /// </summary>
    public string? SubjectContextId { get; set; }

    /// <summary>Last approver on the final step — set by ApprovalWorkflow before FinalizeApprovalAsync.</summary>
    public string? LastApprovedBy { get; set; }
    public string? LastApprovedByName { get; set; }
    public string? LastApprovedById { get; set; }

    /// <summary>
    /// Entity snapshot captured at workflow start (camelCase keys). Persisted on the platform
    /// workflow record for instance-detail UIs — entity-agnostic across all modules.
    /// </summary>
    public Dictionary<string, object> EntityData { get; set; } = new();

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
        CurrentState    = message.CurrentState,
        SubjectContextType = message.SubjectContextType,
        SubjectContextId   = message.SubjectContextId,
        EntityData      = message.EntityData ?? new Dictionary<string, object>()
    };
}
