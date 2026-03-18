namespace QFace.Sdk.Temporal.Abstractions;

/// <summary>
/// Result of a workflow termination or cancellation.
/// </summary>
public sealed class WorkflowTerminationResult
{
    public bool   Success      { get; init; }
    public bool   AlreadyGone  { get; init; } // already completed/terminated — not an error
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Generic workflow terminator for administrative control.
/// Handles stuck, runaway, or abandoned workflows.
///
/// Domain-specific terminators (e.g. IApprovalWorkflowTerminator in Shared.Common)
/// wrap this and additionally update the Platform DB to reflect the termination.
///
/// CancelAsync vs TerminateAsync:
///   Cancel — sends a cancellation request. The workflow receives it and can
///            clean up (compensate, notify) before stopping. Preferred.
///   Terminate — forceful immediate stop. No cleanup. Use for truly stuck workflows
///               that don't respond to cancellation.
/// </summary>
public interface IWorkflowTerminator
{
    /// <summary>
    /// Requests graceful cancellation. The workflow handles the cancellation
    /// and can run compensation logic before stopping.
    /// AlreadyGone=true (not an exception) if workflow is not found.
    /// </summary>
    Task<WorkflowTerminationResult> CancelAsync(
        string workflowId,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Forcefully terminates a workflow immediately.
    /// No cleanup or compensation. Use only for workflows that cannot be cancelled.
    /// AlreadyGone=true (not an exception) if workflow is not found.
    /// </summary>
    Task<WorkflowTerminationResult> TerminateAsync(
        string workflowId,
        string reason,
        CancellationToken cancellationToken = default);
}
