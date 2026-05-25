using Temporalio.Activities;
using QimErp.Shared.Common.Workflow.Entities;

namespace QimErp.Shared.Common.Services.Workflow.Temporal;

/// <summary>
/// Workflow lifecycle email notifications as Temporal activities (retries and history via Temporal).
/// Implemented by <c>NotificationActivity</c> in QimErp.Platform.Workflow.WebApi.
/// </summary>
public interface INotificationActivity
{
    /// <summary>
    /// Notifies the approvers for the given step that their action is required.
    /// </summary>
    [Activity]
    Task NotifyStepApproversAsync(
        ApprovalWorkflowInput input,
        WorkflowStep step,
        WorkflowDefinition definition,
        List<ResolvedApprover> resolvedApprovers);

    /// <summary>
    /// Notifies the original requester that a step was approved and the workflow moved forward.
    /// </summary>
    [Activity]
    Task SendRequesterStepUpdateAsync(
        ApprovalWorkflowInput input,
        WorkflowStep approvedStep,
        ApprovalSignal signal,
        WorkflowDefinition definition,
        bool isLastStep);

    /// <summary>
    /// Notifies all relevant parties that the entire workflow completed successfully.
    /// </summary>
    [Activity]
    Task SendCompletionNotificationAsync(
        ApprovalWorkflowInput input,
        WorkflowDefinition definition);

    /// <summary>
    /// Notifies the requester (and optionally approvers) that the workflow was rejected.
    /// </summary>
    [Activity]
    Task SendRejectionNotificationAsync(
        ApprovalWorkflowInput input,
        WorkflowStep rejectedAtStep,
        ApprovalSignal signal,
        WorkflowDefinition definition);

    /// <summary>
    /// Notifies escalation recipients when a step times out with no response.
    /// </summary>
    [Activity]
    Task SendTimeoutEscalationAsync(
        ApprovalWorkflowInput input,
        WorkflowStep timedOutStep,
        WorkflowDefinition definition);
}
