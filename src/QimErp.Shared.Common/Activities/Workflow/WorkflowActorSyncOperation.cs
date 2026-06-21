namespace QimErp.Shared.Common.Activities.Workflow;

/// <summary>
/// Operation type for a WorkflowActor sync event.
/// Mirrors the shape of <see cref="EmployeeSyncOperation"/> but is source-neutral.
/// </summary>
public enum WorkflowActorSyncOperation
{
    Created = 0,
    Updated = 1,
    Deleted = 2,
}
