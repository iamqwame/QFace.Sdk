namespace QimErp.Shared.Common.Services.Workflow;

/// <summary>
/// Resolved subject context carried through workflow trigger and Temporal input.
/// </summary>
public sealed record WorkflowSubjectContext(string? ContextType, string? ContextId)
{
    public static readonly WorkflowSubjectContext Empty = new(null, null);

    public static WorkflowSubjectContext FromEntity(IWorkflowEnabled entity)
    {
        if (entity is IWorkflowSubjectContextProvider provider)
        {
            return new WorkflowSubjectContext(
                provider.GetWorkflowSubjectContextType(),
                provider.GetWorkflowSubjectContextId());
        }

        return new WorkflowSubjectContext(entity.EntityType, GetEntityId(entity));
    }

    public static WorkflowSubjectContext FromMessage(WorkflowEventMessage message) =>
        new(message.SubjectContextType, message.SubjectContextId);

    public static WorkflowSubjectContext FromInput(Temporal.ApprovalWorkflowInput input) =>
        new(input.SubjectContextType, input.SubjectContextId);

    /// <summary>
    /// Id used for approver resolution (direct_manager, department, etc.).
    /// </summary>
    public string? ResolveApproverSubjectId(string entityId, string entityType) =>
        !string.IsNullOrWhiteSpace(ContextId) ? ContextId : entityId;

    private static string? GetEntityId(IWorkflowEnabled entity) =>
        entity.GetType().GetProperty("Id")?.GetValue(entity)?.ToString();
}
