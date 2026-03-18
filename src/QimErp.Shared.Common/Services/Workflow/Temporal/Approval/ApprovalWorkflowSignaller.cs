using Microsoft.Extensions.Logging;
using QFace.Sdk.Temporal.Abstractions;
using QFace.Sdk.Temporal.Helpers;

namespace QimErp.Shared.Common.Services.Workflow.Temporal.Approval;

internal sealed class ApprovalWorkflowSignaller(
    IWorkflowSignaller signaller,
    ILogger<ApprovalWorkflowSignaller> logger) : IApprovalWorkflowSignaller
{
    // Signal method names must exactly match the [WorkflowSignal] method names
    // defined on ApprovalWorkflow. If these drift, signals silently go nowhere.
    private const string ApproveSignal = "ApproveStepAsync";
    private const string RejectSignal  = "RejectStepAsync";

    public async Task<ApprovalSignalResult> ApproveStepAsync(
        string entityType,
        string entityId,
        ApprovalSignal signal,
        CancellationToken cancellationToken = default)
    {
        var workflowId = TemporalNaming.WorkflowId("approval", entityType, entityId);

        logger.LogInformation(
            "[ApprovalWorkflowSignaller] Sending approve signal. WorkflowId={WorkflowId}, Step={StepCode}",
            workflowId, signal.StepCode);

        var result = await signaller.SendSignalAsync(
            workflowId, ApproveSignal, signal, cancellationToken);

        return new ApprovalSignalResult
        {
            Success      = result.Success,
            WorkflowGone = result.WorkflowGone,
            ErrorMessage = result.ErrorMessage
        };
    }

    public async Task<ApprovalSignalResult> RejectStepAsync(
        string entityType,
        string entityId,
        ApprovalSignal signal,
        CancellationToken cancellationToken = default)
    {
        var workflowId = TemporalNaming.WorkflowId("approval", entityType, entityId);

        logger.LogInformation(
            "[ApprovalWorkflowSignaller] Sending reject signal. WorkflowId={WorkflowId}, Step={StepCode}",
            workflowId, signal.StepCode);

        var result = await signaller.SendSignalAsync(
            workflowId, RejectSignal, signal, cancellationToken);

        return new ApprovalSignalResult
        {
            Success      = result.Success,
            WorkflowGone = result.WorkflowGone,
            ErrorMessage = result.ErrorMessage
        };
    }
}
