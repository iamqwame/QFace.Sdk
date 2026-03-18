namespace QimErp.Shared.Common.Services.Workflow.Temporal.Approval;

/// <summary>
/// Result of an approval workflow termination.
/// </summary>
public sealed class ApprovalWorkflowTerminationResult
{
    public bool   Success      { get; init; }
    public bool   AlreadyGone  { get; init; }
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Typed wrapper over IWorkflowTerminator for approval workflows.
/// Handles stuck, timed-out, or administratively cancelled approval workflows.
///
/// IMPORTANT: Unlike the generic IWorkflowTerminator, this interface also updates
/// the Platform DB to reflect the termination — the workflow record must be
/// marked Cancelled/Rejected and the entity must be updated. This domain logic
/// lives here, not in the SDK.
///
/// CancelAsync — preferred. Sends cancellation request, workflow may compensate.
/// TerminateAsync — forceful. No cleanup. Last resort for stuck workflows.
///
/// Registered in: TemporalServiceCollectionExtensions.AddTemporalWorkflow()
/// </summary>
public interface IApprovalWorkflowTerminator
{
    /// <summary>
    /// Gracefully cancels an in-progress approval workflow.
    /// Updates the Platform DB to reflect cancellation.
    /// The entity is reverted from InProgress to its previous state.
    /// </summary>
    Task<ApprovalWorkflowTerminationResult> CancelAsync(
        string entityType,
        string entityId,
        string reason,
        string cancelledByEmail,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Forcefully terminates a stuck approval workflow.
    /// Updates the Platform DB to reflect termination.
    /// Use only when CancelAsync is not responding.
    /// </summary>
    Task<ApprovalWorkflowTerminationResult> TerminateAsync(
        string entityType,
        string entityId,
        string reason,
        string terminatedByEmail,
        CancellationToken cancellationToken = default);
}
