using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities.Inventory;

/// <summary>
/// Started by AP when a purchase order is confirmed. Fans out to Inventory on-order workers.
/// Task queue: <c>qimerp-accounting-ap-stock-on-order-sync</c>.
/// </summary>
[Workflow("StockOnOrderSyncWorkflow")]
public interface IStockOnOrderSyncWorkflow
{
    [WorkflowRun]
    Task RunAsync(StockOnOrderSyncRequest request);
}
