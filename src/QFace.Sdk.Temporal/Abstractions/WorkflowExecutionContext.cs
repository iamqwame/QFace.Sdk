using Temporalio.Activities;

namespace QFace.Sdk.Temporal.Abstractions;

/// <summary>
/// Provides activity execution context for use inside Temporal activity implementations.
/// Exposes stable values that survive retries and can be used to make
/// activity execution idempotent.
///
/// Usage inside an [Activity] method:
/// <code>
/// var token = WorkflowExecutionContext.Current.IdempotencyToken;
/// if (await repository.WasAlreadyAppliedAsync(token)) return; // idempotent skip
/// await repository.ApplyAsync(token, ...);
/// </code>
/// </summary>
public sealed class WorkflowExecutionContext
{
    private WorkflowExecutionContext() { }

    /// <summary>
    /// Returns the execution context for the currently executing activity.
    /// Must only be called from inside a method decorated with [Activity].
    /// Throws InvalidOperationException if called outside an activity context.
    /// </summary>
    public static WorkflowExecutionContext Current
    {
        get
        {
            // ActivityExecutionContext.Current is set by Temporalio when an activity executes.
            // We access it here to derive a stable, domain-usable context.
            var ctx = ActivityExecutionContext.Current;
            return new WorkflowExecutionContext
            {
                _context = ctx
            };
        }
    }

    private ActivityExecutionContext _context = null!;

    /// <summary>
    /// Stable idempotency token for this specific activity execution.
    /// Derived from WorkflowId + ActivityId — remains the same across retries
    /// of the same activity attempt. Use as a key to detect duplicate execution.
    ///
    /// Format: "{WorkflowId}:{ActivityId}"
    /// Example: "approval-Employee-3f2a1b...:FinalizeApprovalAsync-1"
    /// </summary>
    public string IdempotencyToken =>
        $"{_context.Info.WorkflowId}:{_context.Info.ActivityId}";

    /// <summary>
    /// Current attempt number for this activity (1-based).
    /// 1 = first attempt. 2+ = retry.
    /// Use to add retry-specific logging or skip expensive setup on retries.
    /// </summary>
    public int Attempt => _context.Info.Attempt;

    /// <summary>
    /// True if this is a retry (Attempt > 1).
    /// Convenience property for retry-specific logic.
    /// </summary>
    public bool IsRetry => _context.Info.Attempt > 1;

    /// <summary>
    /// The workflow ID that scheduled this activity.
    /// </summary>
    public string? WorkflowId => _context.Info.WorkflowId;

    /// <summary>
    /// The workflow run ID that scheduled this activity.
    /// </summary>
    public string? WorkflowRunId => _context.Info.WorkflowRunId;

    /// <summary>
    /// The activity type name (method name decorated with [Activity]).
    /// </summary>
    public string ActivityType => _context.Info.ActivityType;

    /// <summary>
    /// Heartbeat to prevent activity timeout on long-running operations.
    /// Call periodically inside long-running [Activity] methods.
    /// </summary>
    public void Heartbeat(params object?[] details)
        => _context.Heartbeat(details);

    /// <summary>
    /// Cancellation token that is cancelled when Temporal requests activity cancellation.
    /// Pass to async operations to support graceful cancellation.
    /// </summary>
    public CancellationToken CancellationToken => _context.CancellationToken;
}
