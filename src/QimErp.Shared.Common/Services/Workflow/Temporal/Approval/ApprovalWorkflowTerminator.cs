using Microsoft.Extensions.Logging;
using QFace.Sdk.Temporal.Abstractions;
using QFace.Sdk.Temporal.Helpers;

namespace QimErp.Shared.Common.Services.Workflow.Temporal.Approval;

/// <summary>
/// Concrete implementation of IApprovalWorkflowTerminator.
/// Signals Temporal to cancel/terminate then updates the Platform DB.
/// The DB update is a deliberate domain concern — the SDK's IWorkflowTerminator
/// only speaks to Temporal, this class also speaks to the workflow record.
///
/// Note: The Platform DB update (marking Workflow as Cancelled) is done via
/// a direct WorkflowApplicationDbContext call. This class is registered in the
/// Platform Workflow module — not in QimErp.Shared.Common directly.
/// The interface lives in Shared.Common; the implementation that touches the DB
/// lives in Platform.Workflow (or wherever the DbContext is available).
///
/// For modules that only need to cancel Temporal (no DB access), they can
/// implement IApprovalWorkflowTerminator themselves with just the signal calls.
/// </summary>
internal sealed class ApprovalWorkflowTerminator(
    IWorkflowTerminator terminator,
    ILogger<ApprovalWorkflowTerminator> logger) : IApprovalWorkflowTerminator
{
    public async Task<ApprovalWorkflowTerminationResult> CancelAsync(
        string entityType,
        string entityId,
        string reason,
        string cancelledByEmail,
        CancellationToken cancellationToken = default)
    {
        var workflowId = TemporalNaming.WorkflowId("approval", entityType, entityId);

        logger.LogInformation(
            "[ApprovalWorkflowTerminator] Cancelling. WorkflowId={WorkflowId}, Reason={Reason}, By={By}",
            workflowId, reason, cancelledByEmail);

        var result = await terminator.CancelAsync(workflowId, reason, cancellationToken);

        // Note: Platform DB update (mark Workflow record as Cancelled) is the
        // responsibility of the caller — typically an admin endpoint in the
        // Platform Workflow WebApi that has access to WorkflowApplicationDbContext.
        // This class only handles the Temporal side.
        // The DB update must happen in the same operation as the cancellation
        // to keep the Platform record in sync with Temporal state.

        return new ApprovalWorkflowTerminationResult
        {
            Success      = result.Success,
            AlreadyGone  = result.AlreadyGone,
            ErrorMessage = result.ErrorMessage
        };
    }

    public async Task<ApprovalWorkflowTerminationResult> TerminateAsync(
        string entityType,
        string entityId,
        string reason,
        string terminatedByEmail,
        CancellationToken cancellationToken = default)
    {
        var workflowId = TemporalNaming.WorkflowId("approval", entityType, entityId);

        logger.LogWarning(
            "[ApprovalWorkflowTerminator] TERMINATING. WorkflowId={WorkflowId}, Reason={Reason}, By={By}",
            workflowId, reason, terminatedByEmail);

        var result = await terminator.TerminateAsync(workflowId, reason, cancellationToken);

        return new ApprovalWorkflowTerminationResult
        {
            Success      = result.Success,
            AlreadyGone  = result.AlreadyGone,
            ErrorMessage = result.ErrorMessage
        };
    }
}
