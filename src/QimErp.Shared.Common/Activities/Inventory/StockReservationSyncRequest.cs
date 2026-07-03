using QimErp.Shared.Common.Events;

namespace QimErp.Shared.Common.Activities.Inventory;

public class StockReservationSyncRequest
{
    public string TenantId { get; set; } = string.Empty;
    public SaleOrderReservedEvent Reserved { get; set; } = null!;
}
