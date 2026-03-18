namespace QimErp.Shared.Common.Services.Workflow.Temporal.Approval;

/// <summary>
/// Result of a workflow state query.
/// </summary>
public sealed class ApprovalWorkflowStateResult
{
    public bool   Success      { get; init; }
    public bool   IsRunning    { get; init; }
    public bool   WorkflowGone { get; init; }
    public string? CurrentState { get; init; }
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Typed wrapper over IWorkflowQueryClient for approval workflows.
/// Allows the UI to read the current approval step without a Platform DB round-trip.
/// Uses the [WorkflowQuery] GetCurrentState() method on IApprovalWorkflow.
///
/// Registered in: TemporalServiceCollectionExtensions.AddTemporalWorkflow()
/// </summary>
public interface IApprovalWorkflowQueryClient
{
    /// <summary>
    /// Returns the current workflow state (step code) for the given entity.
    /// WorkflowGone=true if the workflow is not running (completed, rejected, or not yet started).
    /// </summary>
    Task<ApprovalWorkflowStateResult> GetCurrentStateAsync(
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if an approval workflow is currently running for this entity.
    /// Lightweight — does not read workflow history.
    /// </summary>
    Task<bool> IsRunningAsync(
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default);
}
