namespace QimErp.Shared.Common.Services.Workflow.Temporal.Approval;

/// <summary>
/// Result of an approval workflow signal operation.
/// </summary>
public sealed class ApprovalSignalResult
{
    public bool   Success      { get; init; }
    public bool   WorkflowGone { get; init; }
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Typed wrapper over IWorkflowSignaller for approval workflows.
/// Hides workflow ID format, signal method names, and raw Temporalio exceptions.
///
/// Used by: ApproveWorkflow endpoint, RejectWorkflow endpoint.
/// Replaces: raw temporalClient.GetWorkflowHandle(...).SignalAsync(...) calls.
///
/// WorkflowGone=true means the workflow completed, was terminated, or never started.
/// The caller (endpoint) decides whether this is an error or acceptable.
/// Registered in: TemporalServiceCollectionExtensions.AddTemporalWorkflow()
/// </summary>
public interface IApprovalWorkflowSignaller
{
    /// <summary>
    /// Sends an approval signal for the given step.
    /// Wakes the running ApprovalWorkflow from its WaitConditionAsync.
    /// </summary>
    Task<ApprovalSignalResult> ApproveStepAsync(
        string entityType,
        string entityId,
        ApprovalSignal signal,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a rejection signal for the given step.
    /// Wakes the running ApprovalWorkflow and routes to the rejection path.
    /// </summary>
    Task<ApprovalSignalResult> RejectStepAsync(
        string entityType,
        string entityId,
        ApprovalSignal signal,
        CancellationToken cancellationToken = default);
}
