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
/// Producer modules (today: QimErp.CoreHr.People for employee actors; later:
/// Customers/Vendors/External modules for their respective actors) schedule these
/// activities on Workflow's task queue with a mapped <see cref="WorkflowActorChangedEvent"/>
/// payload. Upserts are keyed by (TenantId, SourceType, SourceId).
/// </summary>
public interface IWorkflowActorSyncActivity
{
    /// <summary>Upsert a newly created actor.</summary>
    [Activity]
    Task SyncActorCreatedAsync(WorkflowActorChangedEvent actor);

    /// <summary>Update an existing actor (idempotent — creates if missing).</summary>
    [Activity]
    Task SyncActorUpdatedAsync(WorkflowActorChangedEvent actor);

    /// <summary>
    /// Mark an actor as inactive in Workflow's projection.
    /// </summary>
    /// <param name="sourceType">Origin system of the actor being deleted.</param>
    /// <param name="sourceId">Source-system primary key.</param>
    /// <param name="tenantId">Tenant scope.</param>
    /// <param name="email">Optional email — fallback identifier for diagnostics.</param>
    [Activity]
    Task SyncActorDeletedAsync(
        WorkflowActorSourceType sourceType,
        Guid sourceId,
        string tenantId,
        string? email);
}
