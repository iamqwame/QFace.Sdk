using QimErp.Shared.Common.Actors;

namespace QimErp.Shared.Common.Services.Workflow;

/// <summary>
/// When registered, the interceptor will call this before publishing workflow approval required via the actor.
/// If this returns true, the event is not published (e.g. Temporal workflow was started instead).
/// </summary>
public interface IWorkflowTriggerBridge
{
    Task<bool> TryTriggerTemporalWorkflowAsync(WorkflowEventMessage message, CancellationToken cancellationToken = default);
}
