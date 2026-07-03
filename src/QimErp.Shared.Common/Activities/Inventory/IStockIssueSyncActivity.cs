using Temporalio.Activities;

namespace QimErp.Shared.Common.Activities.Inventory;

/// <summary>
/// Issues stock and records COGS movement when an invoice ships.
/// Worker queue: <c>qimerp-inventory-stock-issue-sync</c>.
/// </summary>
public interface IStockIssueSyncActivity
{
    [Activity]
    Task ProcessAsync(StockIssueSyncRequest request, CancellationToken cancellationToken = default);
}
