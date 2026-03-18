using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Temporalio.Client;
using Temporalio.Exceptions;
using QFace.Sdk.Temporal.Abstractions;
using Temporalio.Api.Enums.V1;

namespace QFace.Sdk.Temporal.Implementations;

internal sealed class WorkflowStarter(
    ITemporalClient client,
    ILogger<WorkflowStarter> logger) : IWorkflowStarter
{
    public async Task<WorkflowStartResult> StartOrIgnoreAsync<TWorkflow>(
        string workflowId,
        string taskQueue,
        Expression<Func<TWorkflow, Task>> startExpression,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var handle = await client.StartWorkflowAsync(
                startExpression,
                new WorkflowOptions(workflowId, taskQueue)
                {
                    IdConflictPolicy = WorkflowIdConflictPolicy.UseExisting
                });

            logger.LogDebug(
                "[WorkflowStarter] Started or reused workflow. WorkflowId={WorkflowId}, RunId={RunId}",
                workflowId, handle.ResultRunId);

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
        try
        {
            var handle = await client.StartWorkflowAsync(
                startExpression,
                new WorkflowOptions(workflowId, taskQueue)
                {
                    IdConflictPolicy = WorkflowIdConflictPolicy.Fail
                });

            logger.LogDebug(
                "[WorkflowStarter] Started workflow. WorkflowId={WorkflowId}, RunId={RunId}",
                workflowId, handle.ResultRunId);

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
}
