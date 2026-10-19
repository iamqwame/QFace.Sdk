using Temporalio.Activities;

namespace QimErp.Shared.Common.Activities.Workflow;

/// <summary>
/// Activity interface implemented by QimErp.Platform.Workflow.WebApi to sync actor
/// data into its local <c>Actors</c> projection.
///
/// Platform.Workflow registers a Temporal worker on task queue
/// <c>qimerp-platform-workflow-actor-sync</c> and a scoped implementation of this
/// interface.
///
/// Producer modules schedule V2 methods with <see cref="WorkflowActorSyncEnvelope"/>
/// (ids-only). Legacy fat <see cref="WorkflowActorChangedEvent"/> methods remain for
/// backfill / unmigrated callers.
/// </summary>
public interface IWorkflowActorSyncActivity
{
    [Activity]
    Task SyncActorCreatedAsync(WorkflowActorChangedEvent actor);

    [Activity]
    Task SyncActorUpdatedAsync(WorkflowActorChangedEvent actor);

    [Activity]
    Task SyncActorCreatedV2Async(WorkflowActorSyncEnvelope envelope);

    [Activity]
    Task SyncActorUpdatedV2Async(WorkflowActorSyncEnvelope envelope);

    [Activity]
    Task SyncActorDeletedAsync(
        WorkflowActorSourceType sourceType,
        Guid sourceId,
        string tenantId,
        string? email);
}
