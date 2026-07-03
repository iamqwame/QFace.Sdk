using Temporalio.Activities;

namespace QimErp.Shared.Common.Activities.Inventory;

/// <summary>
/// Reserves on-hand stock when a sale order is placed.
/// Worker queue: <c>qimerp-inventory-stock-reservation-sync</c>.
/// </summary>
public interface IStockReservationSyncActivity
{
    [Activity]
    Task ProcessAsync(StockReservationSyncRequest request, CancellationToken cancellationToken = default);
}
