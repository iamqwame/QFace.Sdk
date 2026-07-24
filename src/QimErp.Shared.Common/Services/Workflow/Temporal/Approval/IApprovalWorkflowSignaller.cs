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
/// Used by ApproveWorkflow / RejectWorkflow endpoints; wraps workflow handle signal calls.
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

    /// <summary>
    /// Sends a return-for-edit signal for the given step.
    /// Wakes the running ApprovalWorkflow and routes to the return path — the workflow
    /// ends with WorkflowStatus.Returned; resubmission by the requester starts a fresh instance.
    /// </summary>
    Task<ApprovalSignalResult> ReturnStepAsync(
        string entityType,
        string entityId,
        ApprovalSignal signal,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a reassign signal for the given step, transferring approval responsibility
    /// to <paramref name="newApproverId"/>. The running ApprovalWorkflow may use this to
    /// update its in-memory approver assignment and re-route notifications.
    /// </summary>
    Task<ApprovalSignalResult> ReassignStepAsync(
        string entityType,
        string entityId,
        string stepCode,
        string newApproverId,
        string? comment,
        ApprovalSignal signal,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Signal payload sent when an approval step is reassigned to a different approver.
/// </summary>
public class ReassignSignal
{
    public string StepCode { get; set; } = "";
    public string NewApproverId { get; set; } = "";
    public string? Comment { get; set; }
    public string ReassignedBy { get; set; } = "";
    public string? ReassignedByName { get; set; }
    public string? ReassignedById { get; set; }
    public DateTime ActedAt { get; set; } = DateTime.UtcNow;
}
