using Microsoft.Extensions.Logging;
using Temporalio.Client;
using Temporalio.Exceptions;
using QFace.Sdk.Temporal.Abstractions;

namespace QFace.Sdk.Temporal.Implementations;

internal sealed class WorkflowSignaller(
    ITemporalClient client,
    ILogger<WorkflowSignaller> logger) : IWorkflowSignaller
{
    public async Task<WorkflowSignalResult> SendSignalAsync(
        string workflowId,
        string signalName,
        object payload,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var handle = client.GetWorkflowHandle(workflowId);
            await handle.SignalAsync(signalName, [payload]);

            logger.LogDebug(
                "[WorkflowSignaller] Signal sent. WorkflowId={WorkflowId}, Signal={SignalName}",
                workflowId, signalName);

            return new WorkflowSignalResult { Success = true };
        }
        catch (RpcException ex) when (ex.Code == RpcException.StatusCode.NotFound)
        {
            logger.LogWarning(
                "[WorkflowSignaller] Workflow not found for signal. WorkflowId={WorkflowId}, Signal={SignalName}",
                workflowId, signalName);

            return new WorkflowSignalResult
            {
                Success      = false,
                WorkflowGone = true,
                ErrorMessage = $"Workflow {workflowId} not found."
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "[WorkflowSignaller] Failed to send signal. WorkflowId={WorkflowId}, Signal={SignalName}",
                workflowId, signalName);

            return new WorkflowSignalResult
            {
                Success      = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<WorkflowSignalResult> SendSignalAsync(
        string workflowId,
        string signalName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var handle = client.GetWorkflowHandle(workflowId);
            await handle.SignalAsync(signalName, Array.Empty<object?>());

            logger.LogDebug(
                "[WorkflowSignaller] Signal sent (no payload). WorkflowId={WorkflowId}, Signal={SignalName}",
                workflowId, signalName);

            return new WorkflowSignalResult { Success = true };
        }
        catch (RpcException ex) when (ex.Code == RpcException.StatusCode.NotFound)
        {
            logger.LogWarning(
                "[WorkflowSignaller] Workflow not found. WorkflowId={WorkflowId}, Signal={SignalName}",
                workflowId, signalName);

            return new WorkflowSignalResult
            {
                Success      = false,
                WorkflowGone = true,
                ErrorMessage = $"Workflow {workflowId} not found."
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "[WorkflowSignaller] Failed to send signal. WorkflowId={WorkflowId}, Signal={SignalName}",
                workflowId, signalName);

            return new WorkflowSignalResult
            {
                Success      = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
