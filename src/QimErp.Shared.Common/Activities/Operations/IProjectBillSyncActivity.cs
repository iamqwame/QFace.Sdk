using Temporalio.Activities;

namespace QimErp.Shared.Common.Activities.Operations;

/// <summary>
/// Creates a customer invoice draft from a project bill in AR.
/// Worker queue: <c>qimerp-accounting-ar-project-bill-sync</c>.
/// </summary>
public interface IProjectBillSyncActivity
{
    [Activity]
    Task ProcessAsync(ProjectBillSyncRequest request, CancellationToken cancellationToken = default);
}
