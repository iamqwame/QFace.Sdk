using Temporalio.Activities;

namespace QimErp.Shared.Common.Activities.TenantReference;

/// <summary>
/// Activity implemented by modules that mirror IAM tenant reference data locally.
/// Each module registers a worker on <c>qimerp-{module}-tenant-reference-sync</c>.
/// </summary>
public interface ITenantReferenceSyncActivity
{
    [Activity]
    Task ProcessAsync(TenantReferenceSyncRequest request, CancellationToken cancellationToken = default);
}
