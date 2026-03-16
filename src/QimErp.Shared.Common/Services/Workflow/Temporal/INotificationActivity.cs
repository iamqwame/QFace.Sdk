namespace QimErp.Shared.Common.Services.Workflow.Temporal;

/// <summary>
/// Activity interface for all email / SMS notifications within the workflow lifecycle.
/// Implemented in QimErp.Platform.Workflow.Worker.
///
/// Every method is a Temporal activity — Temporal records its completion and retries on failure.
/// This replaces ALL fire-and-forget notification publishes scattered across:
///   - WorkflowCoreConsumer.SendWorkflowStartNotifications
///   - WorkflowNotificationConsumer.ProcessWorkflowApprovalRequired (all modules)
///   - WorkflowApprovalProcessor.SendNextStepNotificationsAsync
///   - WorkflowApprovalProcessor.SendRequesterNotificationAsync
///   - WorkflowApprovalProcessor.PublishNotificationsAsync
/// </summary>
public interface INotificationActivity
{
    /// <summary>
    /// Notifies the approvers for the given step that their action is required.
    /// Called at the start of every step iteration in the workflow loop.
    /// </summary>
    Task NotifyStepApproversAsync(
        ApprovalWorkflowInput input,
        WorkflowStep step,
        WorkflowDefinition definition);

    /// <summary>
    /// Notifies the original requester that a step was approved and the workflow moved forward.
    /// </summary>
    Task SendRequesterStepUpdateAsync(
        ApprovalWorkflowInput input,
        WorkflowStep approvedStep,
        ApprovalSignal signal,
        WorkflowDefinition definition,
        bool isLastStep);

    /// <summary>
    /// Notifies all relevant parties that the entire workflow completed successfully.
    /// </summary>
    Task SendCompletionNotificationAsync(
        ApprovalWorkflowInput input,
        WorkflowDefinition definition);

    /// <summary>
    /// Notifies the requester (and optionally approvers) that the workflow was rejected.
    /// </summary>
    Task SendRejectionNotificationAsync(
        ApprovalWorkflowInput input,
        WorkflowStep rejectedAtStep,
        ApprovalSignal signal,
        WorkflowDefinition definition);

    /// <summary>
    /// Notifies escalation recipients when a step times out with no response.
    /// </summary>
    Task SendTimeoutEscalationAsync(
        ApprovalWorkflowInput input,
        WorkflowStep timedOutStep,
        WorkflowDefinition definition);
}
