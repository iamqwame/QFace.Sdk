namespace QimErp.Shared.Common.Services.Workflow;

/// <summary>
/// Entity-agnostic helpers for workflow effective/pending state on any <see cref="IWorkflowEnabled"/> row.
/// </summary>
public static class WorkflowEffectiveStatus
{
    /// <summary>
    /// Row is effective for business use (approved or legacy direct write without workflow).
    /// </summary>
    public static bool IsEffective(IWorkflowEnabled entity) =>
        entity.WorkflowStatus is WorkflowStatus.Approved or WorkflowStatus.NotStarted;

    /// <summary>
    /// Row is awaiting approval.
    /// </summary>
    public static bool IsPending(IWorkflowEnabled entity) =>
        entity.WorkflowStatus == WorkflowStatus.InProgress;

    /// <summary>
    /// Combines domain effective flag (when implemented) with workflow status.
    /// </summary>
    public static bool IsBusinessEffective(IWorkflowEnabled entity) =>
        entity is IWorkflowDeferredActivation deferred
            ? deferred.IsEffective
            : IsEffective(entity);
}
