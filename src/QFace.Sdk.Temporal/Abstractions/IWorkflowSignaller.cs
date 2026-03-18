namespace QFace.Sdk.Temporal.Abstractions;

/// <summary>
/// Result of a signal operation.
/// </summary>
public sealed class WorkflowSignalResult
{
    public bool   Success       { get; init; }
    public bool   WorkflowGone  { get; init; } // completed, terminated, or never started
    public string? ErrorMessage  { get; init; }
}

/// <summary>
/// Generic workflow signaller.
/// Wraps ITemporalClient.GetWorkflowHandle(...).SignalAsync(...) with structured
/// error handling so callers never receive raw Temporalio exceptions.
///
/// Domain-specific signallers (e.g. IApprovalWorkflowSignaller in Shared.Common)
/// wrap this with typed signal payloads and fixed signal method names.
/// </summary>
public interface IWorkflowSignaller
{
    /// <summary>
    /// Sends a named signal with a payload to a running workflow.
    /// Returns WorkflowGone=true (not an exception) if the workflow is not found,
    /// already completed, or terminated — callers decide how to handle this.
    /// </summary>
    Task<WorkflowSignalResult> SendSignalAsync(
        string workflowId,
        string signalName,
        object payload,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a signal with no payload (fire-and-forget semantics with result).
    /// </summary>
    Task<WorkflowSignalResult> SendSignalAsync(
        string workflowId,
        string signalName,
        CancellationToken cancellationToken = default);
}
