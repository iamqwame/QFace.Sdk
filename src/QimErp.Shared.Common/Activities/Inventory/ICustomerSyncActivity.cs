using Temporalio.Activities;

namespace QimErp.Shared.Common.Activities.Inventory;

/// <summary>
/// Upserts or deactivates the local Inventory customer cache.
/// Worker queue: <c>qimerp-inventory-customer-sync</c>.
/// </summary>
public interface ICustomerSyncActivity
{
    [Activity]
    Task ProcessAsync(CustomerSyncRequest request, CancellationToken cancellationToken = default);
}
