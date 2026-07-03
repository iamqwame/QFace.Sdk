using Temporalio.Activities;

namespace QimErp.Shared.Common.Activities.Inventory;

/// <summary>
/// Activity implemented by Inventory.WebApi to upsert on-hand quantities when goods are received.
/// Worker queue: <c>qimerp-inventory-stock-sync</c>.
/// </summary>
public interface IStockSyncActivity
{
    [Activity]
    Task ProcessAsync(StockSyncRequest request, CancellationToken cancellationToken = default);
}
