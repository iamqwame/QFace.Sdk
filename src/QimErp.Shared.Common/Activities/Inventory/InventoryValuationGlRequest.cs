namespace QimErp.Shared.Common.Activities.Inventory;

public class InventoryValuationGlRequest
{
    public string TenantId { get; set; } = string.Empty;
    public Guid MovementId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public long WarehouseId { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal? UnitCost { get; set; }
    public decimal TotalValue { get; set; }
    public string? InventoryAccountId { get; set; }
    public string? CogsAccountId { get; set; }
    public string? AdjustmentAccountId { get; set; }
    public string SourceDocType { get; set; } = string.Empty;
    public string SourceDocId { get; set; } = string.Empty;
    public string? SourceDocNumber { get; set; }
    public DateTime MovementDate { get; set; }
}
