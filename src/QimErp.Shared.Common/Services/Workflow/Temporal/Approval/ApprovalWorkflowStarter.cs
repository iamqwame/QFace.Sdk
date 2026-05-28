using QFace.Sdk.Temporal.Abstractions;
using QFace.Sdk.Temporal.Helpers;

namespace QimErp.Shared.Common.Services.Workflow.Temporal.Approval;

internal sealed class ApprovalWorkflowStarter(
    IWorkflowStarter starter,
    ILogger<ApprovalWorkflowStarter> logger) : IApprovalWorkflowStarter
{
    public async Task<ApprovalWorkflowStartResult> StartAsync(
        ApprovalWorkflowInput input,
        CancellationToken cancellationToken = default)
    {
        var workflowId = TemporalNaming.WorkflowId(
            "approval", input.EntityType, input.EntityId);

        logger.LogInformation(
            "[ApprovalWorkflowStarter] Starting. WorkflowId={WorkflowId}, EntityType={EntityType}",
            workflowId, input.EntityType);

        try
        {
            var result = await starter.StartOrIgnoreAsync<IApprovalWorkflow>(
                workflowId,
                TemporalConstants.TaskQueue,
                wf => wf.RunAsync(input),
                cancellationToken);

            if (result.AlreadyRunning)
            {
                logger.LogInformation(
                    "[ApprovalWorkflowStarter] Workflow already running — skipped. WorkflowId={WorkflowId}",
                    workflowId);
            }

            return new ApprovalWorkflowStartResult
            {
                Started        = !result.AlreadyRunning,
                AlreadyRunning = result.AlreadyRunning,
                WorkflowId     = workflowId
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "[ApprovalWorkflowStarter] Failed. WorkflowId={WorkflowId}", workflowId);

            return new ApprovalWorkflowStartResult
            {
                Started       = false,
                WorkflowId    = workflowId,
                ErrorMessage  = ex.Message
            };
        }
    }
}
