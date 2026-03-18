using Microsoft.Extensions.Logging;
using Temporalio.Client;
using Temporalio.Exceptions;
using QFace.Sdk.Temporal.Abstractions;

namespace QFace.Sdk.Temporal.Implementations;

internal sealed class WorkflowQueryClient(
    ITemporalClient client,
    ILogger<WorkflowQueryClient> logger) : IWorkflowQueryClient
{
    public async Task<WorkflowQueryResult<TResult>> QueryAsync<TResult>(
        string workflowId,
        string queryName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var handle = client.GetWorkflowHandle(workflowId);
            var result = await handle.QueryAsync<TResult>(queryName, Array.Empty<object?>());

            logger.LogDebug(
                "[WorkflowQueryClient] Query executed. WorkflowId={WorkflowId}, Query={QueryName}",
                workflowId, queryName);

            return new WorkflowQueryResult<TResult>
            {
                Success = true,
                Value   = result
            };
        }
        catch (RpcException ex) when (ex.Code == RpcException.StatusCode.NotFound)
        {
            logger.LogDebug(
                "[WorkflowQueryClient] Workflow not found. WorkflowId={WorkflowId}, Query={QueryName}",
                workflowId, queryName);

            return new WorkflowQueryResult<TResult>
            {
                Success      = false,
                WorkflowGone = true,
                ErrorMessage = $"Workflow {workflowId} not found."
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "[WorkflowQueryClient] Query failed. WorkflowId={WorkflowId}, Query={QueryName}",
                workflowId, queryName);

            return new WorkflowQueryResult<TResult>
            {
                Success      = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<bool> IsRunningAsync(
        string workflowId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var handle = client.GetWorkflowHandle(workflowId);
            var description = await handle.DescribeAsync();
            return description.Status == Temporalio.Api.Enums.V1.WorkflowExecutionStatus.Running;
        }
        catch (RpcException ex) when (ex.Code == RpcException.StatusCode.NotFound)
        {
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "[WorkflowQueryClient] IsRunning check failed. WorkflowId={WorkflowId}", workflowId);
            return false;
        }
    }
}
