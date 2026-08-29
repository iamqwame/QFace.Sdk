namespace QimErp.Shared.Common.Events;

/// <summary>
/// Published when AP posts a goods receipt. Inventory modules consume via <see cref="Activities.Inventory.IStockSyncWorkflow"/>.
/// </summary>
public class GoodsReceiptPostedEvent : DomainEvent
{
    public Guid GoodsReceiptId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string PurchaseOrderId { get; set; } = string.Empty;
    public string PurchaseOrderCode { get; set; } = string.Empty;
    public string VendorId { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }
    public long? WarehouseId { get; set; }
    public List<GoodsReceiptPostedLineData> Lines { get; set; } = [];

    public GoodsReceiptPostedEvent()
    {
    }

    public GoodsReceiptPostedEvent(
        Guid goodsReceiptId,
        string code,
        string purchaseOrderId,
        string purchaseOrderCode,
        string vendorId,
        string vendorName,
        DateTime receiptDate,
        string tenantId,
        long? warehouseId = null,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        GoodsReceiptId = goodsReceiptId;
        Code = code;
        PurchaseOrderId = purchaseOrderId;
        PurchaseOrderCode = purchaseOrderCode;
        VendorId = vendorId;
        VendorName = vendorName;
        ReceiptDate = receiptDate;
        WarehouseId = warehouseId;
    }

    public static GoodsReceiptPostedEvent Create(
        Guid goodsReceiptId,
        string code,
        string purchaseOrderId,
        string purchaseOrderCode,
        string vendorId,
        string vendorName,
        DateTime receiptDate,
        string tenantId,
        IEnumerable<GoodsReceiptPostedLineData> lines,
        long? warehouseId = null,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new GoodsReceiptPostedEvent(
            goodsReceiptId, code, purchaseOrderId, purchaseOrderCode,
            vendorId, vendorName, receiptDate, tenantId, warehouseId,
            userEmail, triggeredBy, userName)
        {
            Lines = lines.ToList()
        };
    }
}

public class GoodsReceiptPostedLineData
{
    public Guid LineId { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int QuantityReceived { get; set; }
    public decimal UnitPrice { get; set; }
    public string Measurement { get; set; } = string.Empty;
    public Guid? PurchaseOrderLineId { get; set; }
}
