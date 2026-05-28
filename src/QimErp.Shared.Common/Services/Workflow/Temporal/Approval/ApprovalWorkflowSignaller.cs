using QFace.Sdk.Temporal.Abstractions;
using QFace.Sdk.Temporal.Helpers;

namespace QimErp.Shared.Common.Services.Workflow.Temporal.Approval;

internal sealed class ApprovalWorkflowSignaller(
    IWorkflowSignaller signaller,
    ILogger<ApprovalWorkflowSignaller> logger) : IApprovalWorkflowSignaller
{
    // Signal method names must exactly match the [WorkflowSignal] method names
    // defined on ApprovalWorkflow. If these drift, signals silently go nowhere.
    private const string ApproveSignal  = "ApproveStepAsync";
    private const string RejectSignal   = "RejectStepAsync";
    private const string ReassignSignal = "ReassignStepAsync";

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

    public async Task<ApprovalSignalResult> ReassignStepAsync(
        string entityType,
        string entityId,
        string stepCode,
        string newApproverId,
        string? comment,
        ApprovalSignal signal,
        CancellationToken cancellationToken = default)
    {
        var workflowId = TemporalNaming.WorkflowId("approval", entityType, entityId);

        logger.LogInformation(
            "[ApprovalWorkflowSignaller] Sending reassign signal. WorkflowId={WorkflowId}, Step={StepCode}, NewApproverId={NewApproverId}",
            workflowId, stepCode, newApproverId);

        var payload = new ReassignSignal
        {
            StepCode         = stepCode,
            NewApproverId    = newApproverId,
            Comment          = comment,
            ReassignedBy     = signal.ApprovedBy,
            ReassignedByName = signal.ApprovedByName,
            ReassignedById   = signal.ApprovedById,
            ActedAt          = signal.ActedAt
        };

        var result = await signaller.SendSignalAsync(
            workflowId, ReassignSignal, payload, cancellationToken);

        return new ApprovalSignalResult
        {
            Success      = result.Success,
            WorkflowGone = result.WorkflowGone,
            ErrorMessage = result.ErrorMessage
        };
    }
}
