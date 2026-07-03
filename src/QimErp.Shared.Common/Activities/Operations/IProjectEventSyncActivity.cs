using Temporalio.Activities;

namespace QimErp.Shared.Common.Activities.Operations;

/// <summary>
/// Activity implemented by modules that maintain a local Project read model.
/// Each module registers a worker on <c>qimerp-{module}-project-sync</c>.
/// </summary>
public interface IProjectEventSyncActivity
{
    [Activity]
    Task ProcessAsync(ProjectEventSyncRequest request, CancellationToken cancellationToken = default);
}
