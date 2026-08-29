namespace QimErp.Shared.Common.Activities.Inventory;

public class StockOnOrderSyncRequest
{
    public string TenantId { get; set; } = string.Empty;
    public PurchaseOrderOnOrderData OnOrder { get; set; } = null!;
}

public class PurchaseOrderOnOrderData
{
    public Guid PurchaseOrderId { get; set; }
    public string PurchaseOrderCode { get; set; } = string.Empty;
    public string VendorId { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public long? WarehouseId { get; set; }
    public List<PurchaseOrderOnOrderLineData> Lines { get; set; } = [];
}

public class PurchaseOrderOnOrderLineData
{
    public Guid LineId { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public Guid? PurchaseOrderLineId { get; set; }
}
