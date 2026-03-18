namespace QimErp.Shared.Common.Services.Workflow.Temporal.Approval;

/// <summary>
/// Domain-specific result for approval workflow start operations.
/// </summary>
public sealed class ApprovalWorkflowStartResult
{
    public bool   Started        { get; init; }
    public bool   AlreadyRunning { get; init; }
    public string WorkflowId     { get; init; } = "";
    public string? ErrorMessage  { get; init; }
}

/// <summary>
/// Typed wrapper over IWorkflowStarter for approval workflows.
/// Hides workflow ID format, task queue, and conflict policy from callers.
/// Used by TemporalWorkflowTriggerBridge — replaces raw ITemporalClient.StartWorkflowAsync.
///
/// Registered in: TemporalServiceCollectionExtensions.AddTemporalWorkflow()
/// </summary>
public interface IApprovalWorkflowStarter
{
    /// <summary>
    /// Starts an approval workflow for the given entity.
    /// Idempotent — if a workflow is already running for this entity, returns AlreadyRunning=true.
    /// This is the correct policy for the interceptor path where duplicate saves can occur.
    /// </summary>
    Task<ApprovalWorkflowStartResult> StartAsync(
        ApprovalWorkflowInput input,
        CancellationToken cancellationToken = default);
}
