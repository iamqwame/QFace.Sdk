using QimErp.Shared.Common.Services.Workflow.Temporal;

namespace QimErp.Shared.Common.Services.Workflow;

/// <summary>
/// Module-registered handler for domain side-effects when a workflow subject advances,
/// completes, rejects, or times out. One handler per entity type within a module.
/// </summary>
public interface IWorkflowEntityApprovalHandler
{
    string EntityType { get; }

    Task OnAdvanceAsync(
        ApprovalWorkflowInput input,
        WorkflowStep approvedStep,
        WorkflowStep nextStep,
        ApprovalSignal signal,
        CancellationToken cancellationToken = default);

    Task OnFinalizeAsync(
        ApprovalWorkflowInput input,
        CancellationToken cancellationToken = default);

    Task OnRejectAsync(
        ApprovalWorkflowInput input,
        WorkflowStep rejectedAtStep,
        ApprovalSignal signal,
        CancellationToken cancellationToken = default);

    Task OnTimeoutAsync(
        ApprovalWorkflowInput input,
        WorkflowStep timedOutStep,
        CancellationToken cancellationToken = default);
}
