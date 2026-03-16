namespace QimErp.Shared.Common.Services.Workflow.Temporal;

/// <summary>
/// Temporal activity interface that dispatches entity-specific workflow outcomes
/// to the correct module via HTTP.
///
/// Implemented by ModuleApprovalDispatcherActivity in QimErp.Platform.Orchestration.WebApi.
/// Routes by ApprovalWorkflowInput.EntityType to the corresponding module's internal API.
///
/// This is the activity called FROM ApprovalWorkflow — each method is a [Activity].
/// It is NOT implemented by individual modules; for module-side activity interfaces see IModuleApprovalActivity.
/// </summary>
public interface IModuleApprovalDispatcherActivity
{
    /// <summary>
    /// Called when the final workflow step is approved.
    /// Dispatches to the module to activate the entity (e.g. ActivateFromDraft).
    /// </summary>
    Task FinalizeApprovalAsync(ApprovalWorkflowInput input);

    /// <summary>
    /// Called when an intermediate step is approved.
    /// Dispatches to the module to advance entity.CurrentWorkflowState to the next step.
    /// </summary>
    Task AdvanceEntityStepAsync(
        ApprovalWorkflowInput input,
        WorkflowStep approvedStep,
        WorkflowStep nextStep,
        ApprovalSignal signal);

    /// <summary>
    /// Called when the workflow is rejected at any step.
    /// Dispatches to the module to set entity WorkflowStatus to Rejected.
    /// </summary>
    Task RejectEntityAsync(
        ApprovalWorkflowInput input,
        WorkflowStep rejectedAtStep,
        ApprovalSignal signal);

    /// <summary>
    /// Called when a step times out with no approver response.
    /// Dispatches to the module — behaviour (auto-reject vs escalate) is module-specific.
    /// </summary>
    Task TimeoutEntityAsync(ApprovalWorkflowInput input, WorkflowStep timedOutStep);
}
