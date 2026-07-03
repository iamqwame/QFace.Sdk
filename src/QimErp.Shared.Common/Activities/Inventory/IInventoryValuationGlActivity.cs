using Temporalio.Activities;

namespace QimErp.Shared.Common.Activities.Inventory;

/// <summary>
/// Posts inventory valuation journal entries in Core GL.
/// Worker queue: <c>qimerp-accounting-gl-inventory-valuation</c>.
/// </summary>
public interface IInventoryValuationGlActivity
{
    [Activity]
    Task ProcessAsync(InventoryValuationGlRequest request, CancellationToken cancellationToken = default);
}
