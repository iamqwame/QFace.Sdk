using QimErp.Shared.Common.Events;

namespace QimErp.Shared.Common.Activities.Operations;

public enum ProjectEventOperation
{
    Created,
    Updated,
    Deleted
}

/// <summary>
/// Payload for <see cref="IProjectEventSyncWorkflow"/> fan-out activities.
/// Operations Project starts the workflow after project CRUD; downstream modules
/// maintain local project read models via <see cref="IProjectEventSyncActivity"/>.
/// </summary>
public class ProjectEventSyncRequest
{
    public ProjectEventOperation Operation { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public ProjectCreatedEvent? Created { get; set; }
    public ProjectUpdatedEvent? Updated { get; set; }
    public ProjectDeletedEvent? Deleted { get; set; }
}
