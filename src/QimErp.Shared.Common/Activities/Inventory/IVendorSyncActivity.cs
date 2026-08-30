using Temporalio.Activities;

namespace QimErp.Shared.Common.Activities.Inventory;

/// <summary>
/// Upserts or deactivates the local Inventory vendor cache.
/// Worker queue: <c>qimerp-inventory-vendor-sync</c>.
/// </summary>
public interface IVendorSyncActivity
{
    [Activity]
    Task ProcessAsync(VendorSyncRequest request, CancellationToken cancellationToken = default);
}
