namespace QimErp.Shared.Common.Services.Workflow.Temporal;

/// <summary>
/// Activity interface that each module implements to apply workflow outcomes
/// to its own entities in its own database.
///
/// This is the ONLY interface each module needs to implement to participate
/// in the Temporal workflow.  It replaces:
///   - WorkflowConsumer.ProcessWorkflowApprovalRequest   (Employee, Recruitment, Leave, …)
///   - WorkflowNotificationConsumer.ProcessWorkflowApprovalRequest
///   - WorkflowApprovalProcessor.ProcessApprovalRequestAsync (final-step activation)
///   - WorkflowRejectionProcessor.ProcessRejectionRequestAsync
///
/// Implementations live in each module's Consumer project and are registered
/// in the module's Program.cs via services.AddModuleApprovalActivity&lt;T&gt;(configuration, module, entityTypes).
/// Each module Consumer runs its own Temporal worker polling "qimerp-{module}-approvals",
/// so activity type names never collide across modules.
/// </summary>
public interface IModuleApprovalActivity
{
    /// <summary>
    /// Called when the final workflow step is approved.
    /// Must activate the entity (ActivateFromDraft), set completion fields,
    /// and save to the module DB.
    /// </summary>
    Task FinalizeApprovalAsync(ApprovalWorkflowInput input);

    /// <summary>
    /// Called when any step is approved but the workflow is not yet complete.
    /// Must advance entity.CurrentWorkflowState to the next step and save.
    /// </summary>
    Task AdvanceEntityStepAsync(
        ApprovalWorkflowInput input,
        WorkflowStep approvedStep,
        WorkflowStep nextStep,
        ApprovalSignal signal);

    /// <summary>
    /// Called when the workflow is rejected at any step.
    /// Must set entity WorkflowStatus to Rejected and save.
    /// </summary>
    Task RejectEntityAsync(
        ApprovalWorkflowInput input,
        WorkflowStep rejectedAtStep,
        ApprovalSignal signal);

    /// <summary>
    /// Called when a step times out with no approver response.
    /// Behaviour (auto-reject vs escalate) is module-specific.
    /// </summary>
    Task TimeoutEntityAsync(ApprovalWorkflowInput input, WorkflowStep timedOutStep);
}
