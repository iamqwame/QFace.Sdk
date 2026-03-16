using Temporalio.Api.Enums.V1;
using Temporalio.Client;

namespace QimErp.Shared.Common.Services.Workflow.Temporal;

/// <summary>
/// Implements IWorkflowTriggerBridge using Temporal.
/// When registered (i.e. Temporal:Address is configured), the interceptor
/// calls this instead of publishing via WorkflowEventPublisherActor.
/// Returning true tells the interceptor to skip the actor path entirely.
///
/// Registration: call services.AddTemporalWorkflow(configuration) explicitly in the module's
/// Program.cs, or use the AddDbContextWithOutboxAndTemporal convenience wrapper.
/// Note: AddDbContextWithOutbox does NOT call this automatically.
/// </summary>
public sealed class TemporalWorkflowTriggerBridge(ITemporalClient client) : IWorkflowTriggerBridge
{
    public async Task<bool> TryTriggerTemporalWorkflowAsync(
        WorkflowEventMessage message,
        CancellationToken cancellationToken = default)
    {
        var workflowId = TemporalConstants.WorkflowId(message.EntityType, message.EntityId);

        await client.StartWorkflowAsync<IApprovalWorkflow>(
            wf => wf.RunAsync(ApprovalWorkflowInput.From(message)),
            new WorkflowOptions(workflowId, TemporalConstants.TaskQueue)
            {
                // IdConflictPolicy.Fail means a second save on the same entity
                // while a workflow is already running will throw — intentional:
                // the entity should not be re-editable while InProgress.
                IdConflictPolicy = WorkflowIdConflictPolicy.Fail
            });

        return true; // interceptor skips WorkflowEventPublisherActor
    }
}
