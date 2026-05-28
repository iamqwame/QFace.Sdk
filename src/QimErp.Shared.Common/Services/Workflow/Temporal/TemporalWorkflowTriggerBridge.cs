using QimErp.Shared.Common.Services.Workflow.Temporal.Approval;

namespace QimErp.Shared.Common.Services.Workflow.Temporal;

/// <summary>
/// Implements IWorkflowTriggerBridge using Temporal.
/// Called by AuditEntitySaveChangesInterceptor when an IWorkflowEnabled entity is saved.
///
/// Uses IApprovalWorkflowStarter (not raw ITemporalClient) for:
///   - Stable workflow ID format (TemporalNaming.WorkflowId — no string drift)
///   - Idempotent start (StartOrIgnoreAsync — duplicate saves don't throw)
///   - Structured result (AlreadyRunning=true, not an exception)
/// </summary>
public sealed class TemporalWorkflowTriggerBridge(
    IApprovalWorkflowStarter starter,
    ILogger<TemporalWorkflowTriggerBridge> logger) : IWorkflowTriggerBridge
{
    public async Task<bool> TryTriggerTemporalWorkflowAsync(
        WorkflowEventMessage message,
        CancellationToken cancellationToken = default)
    {
        var input = ApprovalWorkflowInput.From(message);
        var result = await starter.StartAsync(input, cancellationToken);

        if (!result.Started && !result.AlreadyRunning)
        {
            // StartAsync caught an exception and returned a failure result.
            logger.LogError(
                "[TemporalWorkflowTriggerBridge] Failed to start workflow. " +
                "EntityType={EntityType}, EntityId={EntityId}, Error={Error}.",
                message.EntityType, message.EntityId, result.ErrorMessage);

            return false;
        }

        logger.LogInformation(
            "[TemporalWorkflowTriggerBridge] Temporal workflow {Status}. " +
            "WorkflowId={WorkflowId}, EntityType={EntityType}, EntityId={EntityId}",
            result.AlreadyRunning ? "already running (skipped)" : "started",
            result.WorkflowId, message.EntityType, message.EntityId);

        return true;
    }
}
