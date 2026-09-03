using System.Linq.Expressions;

namespace QFace.Sdk.Temporal.Abstractions;

/// <summary>
/// Result of a workflow start operation.
/// </summary>
public sealed class WorkflowStartResult
{
    public string WorkflowId  { get; init; } = "";
    public string RunId       { get; init; } = "";

    /// <summary>
    /// True when <see cref="IWorkflowStarter.StartOrIgnoreAsync{TWorkflow}"/> found the workflow
    /// already running. Always false for <see cref="IWorkflowStarter.SignalWithStartAsync{TWorkflow}"/> —
    /// the Temporalio 1.14.1 client discards the SignalWithStartWorkflowExecution response's
    /// "started" flag, so that call cannot tell a new run from an existing one.
    /// </summary>
    public bool   AlreadyRunning { get; init; }
}

/// <summary>
/// Generic idempotent workflow starter.
/// Hides raw ITemporalClient.StartWorkflowAsync behind a clean abstraction
/// that handles conflict policy and maps Temporal exceptions to structured results.
///
/// Caller passes the workflow run expression, e.g. wf => wf.RunAsync(myInput).
/// Domain-specific starters (e.g. IApprovalWorkflowStarter in Shared.Common)
/// wrap this with typed input and fixed workflow type.
/// </summary>
public interface IWorkflowStarter
{
    /// <summary>
    /// Starts a workflow. If a workflow with the same ID is already running,
    /// returns AlreadyRunning=true without throwing.
    /// Use for idempotent start — interceptor retry, duplicate save, etc.
    /// </summary>
    /// <param name="startExpression">e.g. wf => wf.RunAsync(myInput)</param>
    Task<WorkflowStartResult> StartOrIgnoreAsync<TWorkflow>(
        string workflowId,
        string taskQueue,
        Expression<Func<TWorkflow, Task>> startExpression,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts a workflow. If a workflow with the same ID is already running,
    /// throws WorkflowAlreadyStartedException.
    /// Use when duplicate start is a genuine error (e.g. admin triggered twice).
    /// </summary>
    /// <param name="startExpression">e.g. wf => wf.RunAsync(myInput)</param>
    Task<WorkflowStartResult> StartOrRaiseAsync<TWorkflow>(
        string workflowId,
        string taskQueue,
        Expression<Func<TWorkflow, Task>> startExpression,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically starts a workflow if it is not already running and delivers a signal to it,
    /// in a single server call. If it is already running, only the signal is delivered.
    /// Use when the signal must never be lost — a start followed by a separate signal drops
    /// the signal when a concurrent caller wins the start race.
    /// The returned AlreadyRunning is always false; see <see cref="WorkflowStartResult.AlreadyRunning"/>.
    /// </summary>
    /// <param name="startExpression">e.g. wf => wf.RunAsync(myInput)</param>
    /// <param name="signalExpression">e.g. wf => wf.EnqueueAsync(myRequest)</param>
    Task<WorkflowStartResult> SignalWithStartAsync<TWorkflow>(
        string workflowId,
        string taskQueue,
        Expression<Func<TWorkflow, Task>> startExpression,
        Expression<Func<TWorkflow, Task>> signalExpression,
        CancellationToken cancellationToken = default);
}
