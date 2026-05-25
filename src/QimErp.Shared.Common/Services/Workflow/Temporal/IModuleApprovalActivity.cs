using Temporalio.Activities;

namespace QimErp.Shared.Common.Services.Workflow.Temporal;

/// <summary>
/// Module-implemented Temporal activities: apply approval workflow outcomes to the module's own database.
/// </summary>
public interface IModuleApprovalActivity
{
    [Activity]
    Task FinalizeApprovalAsync(ApprovalWorkflowInput input);

    [Activity]
    Task AdvanceEntityStepAsync(
        ApprovalWorkflowInput input,
        WorkflowStep approvedStep,
        WorkflowStep nextStep,
        ApprovalSignal signal);

    [Activity]
    Task RejectEntityAsync(
        ApprovalWorkflowInput input,
        WorkflowStep rejectedAtStep,
        ApprovalSignal signal);

    [Activity]
    Task TimeoutEntityAsync(ApprovalWorkflowInput input, WorkflowStep timedOutStep);
}
