using Microsoft.Extensions.Logging;
using Temporalio.Client;
using Temporalio.Exceptions;
using QFace.Sdk.Temporal.Abstractions;

namespace QFace.Sdk.Temporal.Implementations;

internal sealed class WorkflowTerminator(
    ITemporalClient client,
    ILogger<WorkflowTerminator> logger) : IWorkflowTerminator
{
    public async Task<WorkflowTerminationResult> CancelAsync(
        string workflowId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var handle = client.GetWorkflowHandle(workflowId);
            await handle.CancelAsync();

            logger.LogInformation(
                "[WorkflowTerminator] Cancellation requested. WorkflowId={WorkflowId}, Reason={Reason}",
                workflowId, reason);

            return new WorkflowTerminationResult { Success = true };
        }
        catch (RpcException ex) when (ex.Code == RpcException.StatusCode.NotFound)
        {
            logger.LogWarning(
                "[WorkflowTerminator] Workflow not found for cancel. WorkflowId={WorkflowId}",
                workflowId);

            return new WorkflowTerminationResult { Success = true, AlreadyGone = true };
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "[WorkflowTerminator] Cancel failed. WorkflowId={WorkflowId}", workflowId);

            return new WorkflowTerminationResult
            {
                Success      = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<WorkflowTerminationResult> TerminateAsync(
        string workflowId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var handle = client.GetWorkflowHandle(workflowId);
            await handle.TerminateAsync(reason);

            logger.LogWarning(
                "[WorkflowTerminator] Workflow TERMINATED. WorkflowId={WorkflowId}, Reason={Reason}",
                workflowId, reason);

            return new WorkflowTerminationResult { Success = true };
        }
        catch (RpcException ex) when (ex.Code == RpcException.StatusCode.NotFound)
        {
            logger.LogWarning(
                "[WorkflowTerminator] Workflow not found for terminate. WorkflowId={WorkflowId}",
                workflowId);

            return new WorkflowTerminationResult { Success = true, AlreadyGone = true };
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "[WorkflowTerminator] Terminate failed. WorkflowId={WorkflowId}", workflowId);

            return new WorkflowTerminationResult
            {
                Success      = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
