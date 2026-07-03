using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities.Inventory;

/// <summary>
/// Started by Inventory when a valuation-impacting movement occurs. Fans out to Core GL.
/// Task queue: <c>qimerp-operations-inventory-valuation-gl</c>.
/// </summary>
[Workflow("InventoryValuationGlWorkflow")]
public interface IInventoryValuationGlWorkflow
{
    [WorkflowRun]
    Task RunAsync(InventoryValuationGlRequest request);
}
