using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Temporalio.Client;
using Temporalio.Exceptions;
using QFace.Sdk.Temporal.Abstractions;
using QFace.Sdk.Temporal.Options;
using Temporalio.Api.Enums.V1;

namespace QFace.Sdk.Temporal.Implementations;

internal sealed class WorkflowStarter(
    ITemporalClient client,
    IOptions<TemporalOptions> options,
    ILogger<WorkflowStarter> logger) : IWorkflowStarter
{
    private readonly TemporalOptions _opts = options.Value;

    public async Task<WorkflowStartResult> StartOrIgnoreAsync<TWorkflow>(
        string workflowId,
        string taskQueue,
        Expression<Func<TWorkflow, Task>> startExpression,
        CancellationToken cancellationToken = default)
    {
        // Callers pass the BASE queue name; the SDK appends Temporal:TaskQueueSuffix
        // so each environment routes to its own workers without coordination.
        var resolvedTaskQueue = _opts.WithTaskQueueSuffix(taskQueue);

        try
        {
            var handle = await client.StartWorkflowAsync(
                startExpression,
                new WorkflowOptions(workflowId, resolvedTaskQueue)
                {
                    IdConflictPolicy = WorkflowIdConflictPolicy.UseExisting
                });

            logger.LogDebug(
                "[WorkflowStarter] Started or reused workflow. WorkflowId={WorkflowId}, RunId={RunId}, TaskQueue={TaskQueue}",
                workflowId, handle.ResultRunId, resolvedTaskQueue);

            return new WorkflowStartResult
            {
                WorkflowId     = workflowId,
                RunId          = handle.ResultRunId ?? "",
                AlreadyRunning = false
            };
        }
        catch (WorkflowAlreadyStartedException ex)
        {
            logger.LogInformation(
                "[WorkflowStarter] Workflow already running — skipped. WorkflowId={WorkflowId}",
                workflowId);

            return new WorkflowStartResult
            {
                WorkflowId     = workflowId,
                RunId          = ex.RunId ?? "",
                AlreadyRunning = true
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "[WorkflowStarter] Failed to start workflow. WorkflowId={WorkflowId}", workflowId);
            throw;
        }
    }

    public async Task<WorkflowStartResult> StartOrRaiseAsync<TWorkflow>(
        string workflowId,
        string taskQueue,
        Expression<Func<TWorkflow, Task>> startExpression,
        CancellationToken cancellationToken = default)
    {
        var resolvedTaskQueue = _opts.WithTaskQueueSuffix(taskQueue);

        try
        {
            var handle = await client.StartWorkflowAsync(
                startExpression,
                new WorkflowOptions(workflowId, resolvedTaskQueue)
                {
                    IdConflictPolicy = WorkflowIdConflictPolicy.Fail
                });

            logger.LogDebug(
                "[WorkflowStarter] Started workflow. WorkflowId={WorkflowId}, RunId={RunId}, TaskQueue={TaskQueue}",
                workflowId, handle.ResultRunId, resolvedTaskQueue);

            return new WorkflowStartResult
            {
                WorkflowId     = workflowId,
                RunId          = handle.ResultRunId ?? "",
                AlreadyRunning = false
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "[WorkflowStarter] Failed to start workflow. WorkflowId={WorkflowId}", workflowId);
            throw;
        }
    }

    public async Task<WorkflowStartResult> SignalWithStartAsync<TWorkflow>(
        string workflowId,
        string taskQueue,
        Expression<Func<TWorkflow, Task>> startExpression,
        Expression<Func<TWorkflow, Task>> signalExpression,
        CancellationToken cancellationToken = default)
    {
        var resolvedTaskQueue = _opts.WithTaskQueueSuffix(taskQueue);

        // WorkflowIdConflictPolicy.Fail is rejected by the server on signal-with-start.
        var workflowOptions = new WorkflowOptions(workflowId, resolvedTaskQueue)
        {
            IdConflictPolicy = WorkflowIdConflictPolicy.UseExisting
        };
        workflowOptions.SignalWithStart(signalExpression);

        try
        {
            var handle = await client.StartWorkflowAsync(startExpression, workflowOptions);

            logger.LogDebug(
                "[WorkflowStarter] Signalled workflow with start. WorkflowId={WorkflowId}, RunId={RunId}, TaskQueue={TaskQueue}",
                workflowId, handle.ResultRunId, resolvedTaskQueue);

            return new WorkflowStartResult
            {
                WorkflowId     = workflowId,
                RunId          = handle.ResultRunId ?? "",
                AlreadyRunning = false
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "[WorkflowStarter] Failed to signal workflow with start. WorkflowId={WorkflowId}", workflowId);
            throw;
        }
    }
}
