namespace QimErp.Shared.Common.Services.Workflow.Temporal;

/// <summary>
/// Module-implemented Temporal activities: apply approval workflow outcomes to the module's own database.
/// Register implementations in each module's <c>Program.cs</c> with
/// <c>services.AddModuleApprovalActivity&lt;T&gt;(configuration, module, entityTypes)</c>.
/// Each module runs a worker on <c>qimerp-{module}-approvals</c> so activity type names do not collide.
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
