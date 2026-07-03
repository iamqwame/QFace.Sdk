using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities.Inventory;

/// <summary>
/// Temporal workflow started by AP when a goods receipt is posted.
/// Fans out to module stock-sync workers (Inventory).
/// Task queue: <c>qimerp-accounting-ap-stock-sync</c>.
/// </summary>
[Workflow("StockSyncWorkflow")]
public interface IStockSyncWorkflow
{
    [WorkflowRun]
    Task RunAsync(StockSyncRequest request);
}
