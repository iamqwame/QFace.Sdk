using QimErp.Shared.Common.Events;

namespace QimErp.Shared.Common.Activities.Inventory;

public class StockIssueSyncRequest
{
    public string TenantId { get; set; } = string.Empty;
    public InvoiceShippedEvent Shipped { get; set; } = null!;
}
