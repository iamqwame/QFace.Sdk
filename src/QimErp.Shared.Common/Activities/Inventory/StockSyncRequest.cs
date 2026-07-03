using QimErp.Shared.Common.Events;

namespace QimErp.Shared.Common.Activities.Inventory;

public class StockSyncRequest
{
    public string TenantId { get; set; } = string.Empty;
    public GoodsReceiptPostedEvent Posted { get; set; } = null!;
}
