using Temporalio.Activities;

namespace QimErp.Shared.Common.Activities.Inventory;

/// <summary>
/// Increments on-order quantities when a purchase order is placed.
/// Worker queue: <c>qimerp-inventory-stock-on-order-sync</c>.
/// </summary>
public interface IStockOnOrderSyncActivity
{
    [Activity]
    Task ProcessAsync(StockOnOrderSyncRequest request, CancellationToken cancellationToken = default);
}
