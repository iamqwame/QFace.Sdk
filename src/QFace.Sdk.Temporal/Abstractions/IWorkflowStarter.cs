using System.Linq.Expressions;

namespace QFace.Sdk.Temporal.Abstractions;

/// <summary>
/// Result of a workflow start operation.
/// </summary>
public sealed class WorkflowStartResult
{
    public string WorkflowId  { get; init; } = "";
    public string RunId       { get; init; } = "";
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
}
