using QimErp.Shared.Common.Events;

namespace QimErp.Shared.Common.Activities.Inventory;

public class StockIssueSyncRequest
{
    public string TenantId { get; set; } = string.Empty;
    public InvoiceShippedEvent Shipped { get; set; } = null!;

    /// <summary>Resolved by orchestrator from IAM; activities no-op when target module is not installed.</summary>
    public List<string>? SelectedModules { get; set; }
}
