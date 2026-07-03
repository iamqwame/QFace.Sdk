using Temporalio.Activities;

namespace QimErp.Shared.Common.Activities.Operations;

/// <summary>
/// Activity implemented by Project.WebApi to capture AP bill costs as project expenditures.
/// Worker queue: <c>qimerp-operations-project-expenditure-sync</c>.
/// </summary>
public interface IProjectExpenditureSyncActivity
{
    [Activity]
    Task ProcessAsync(ProjectExpenditureSyncRequest request, CancellationToken cancellationToken = default);
}
