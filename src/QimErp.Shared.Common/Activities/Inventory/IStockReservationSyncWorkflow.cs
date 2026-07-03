using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities.Inventory;

/// <summary>
/// Started by AR when a sale order is created. Fans out to Inventory reservation workers.
/// Task queue: <c>qimerp-accounting-ar-stock-reservation-sync</c>.
/// </summary>
[Workflow("StockReservationSyncWorkflow")]
public interface IStockReservationSyncWorkflow
{
    [WorkflowRun]
    Task RunAsync(StockReservationSyncRequest request);
}
