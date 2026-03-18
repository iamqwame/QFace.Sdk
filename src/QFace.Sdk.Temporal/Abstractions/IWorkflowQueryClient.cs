namespace QFace.Sdk.Temporal.Abstractions;

/// <summary>
/// Result of a workflow query.
/// </summary>
public sealed class WorkflowQueryResult<T>
{
    public bool   Success      { get; init; }
    public T?     Value        { get; init; }
    public bool   WorkflowGone { get; init; }
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Generic workflow query client.
/// Wraps ITemporalClient.GetWorkflowHandle(...).QueryAsync(...) with structured
/// error handling — callers never deal with raw Temporalio exceptions.
///
/// Domain-specific query clients (e.g. IApprovalWorkflowQueryClient in Shared.Common)
/// wrap this with fixed query method names and return types.
/// </summary>
public interface IWorkflowQueryClient
{
    /// <summary>
    /// Queries a running workflow by method name.
    /// Returns WorkflowGone=true if the workflow is not found or already completed.
    /// Callers decide whether WorkflowGone is an error or expected.
    /// </summary>
    Task<WorkflowQueryResult<TResult>> QueryAsync<TResult>(
        string workflowId,
        string queryName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if a workflow with this ID is currently running.
    /// Uses a lightweight existence check without reading workflow history.
    /// </summary>
    Task<bool> IsRunningAsync(
        string workflowId,
        CancellationToken cancellationToken = default);
}
