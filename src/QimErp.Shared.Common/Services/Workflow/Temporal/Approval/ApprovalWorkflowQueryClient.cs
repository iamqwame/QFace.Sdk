using QFace.Sdk.Temporal.Abstractions;
using QFace.Sdk.Temporal.Helpers;

namespace QimErp.Shared.Common.Services.Workflow.Temporal.Approval;

internal sealed class ApprovalWorkflowQueryClient(
    IWorkflowQueryClient queryClient,
    ILogger<ApprovalWorkflowQueryClient> logger) : IApprovalWorkflowQueryClient
{
    // Must match the [WorkflowQuery] method name on ApprovalWorkflow exactly.
    private const string GetCurrentStateQuery = "GetCurrentState";

    public async Task<ApprovalWorkflowStateResult> GetCurrentStateAsync(
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default)
    {
        var workflowId = TemporalNaming.WorkflowId("approval", entityType, entityId);

        var result = await queryClient.QueryAsync<string>(
            workflowId, GetCurrentStateQuery, cancellationToken);

        if (result.WorkflowGone)
        {
            logger.LogDebug(
                "[ApprovalWorkflowQueryClient] Workflow not running. WorkflowId={WorkflowId}",
                workflowId);

            return new ApprovalWorkflowStateResult
            {
                Success      = true,
                IsRunning    = false,
                WorkflowGone = true
            };
        }

        if (!result.Success)
        {
            logger.LogWarning(
                "[ApprovalWorkflowQueryClient] Query failed. WorkflowId={WorkflowId}, Error={Error}",
                workflowId, result.ErrorMessage);

            return new ApprovalWorkflowStateResult
            {
                Success      = false,
                ErrorMessage = result.ErrorMessage
            };
        }

        return new ApprovalWorkflowStateResult
        {
            Success      = true,
            IsRunning    = true,
            CurrentState = result.Value
        };
    }

    public Task<bool> IsRunningAsync(
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default)
    {
        var workflowId = TemporalNaming.WorkflowId("approval", entityType, entityId);
        return queryClient.IsRunningAsync(workflowId, cancellationToken);
    }
}
