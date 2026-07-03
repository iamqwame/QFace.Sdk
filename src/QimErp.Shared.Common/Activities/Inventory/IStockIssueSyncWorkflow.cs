using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities.Inventory;

/// <summary>
/// Started by AR when an invoice is shipped. Fans out to Inventory stock-issue workers.
/// Task queue: <c>qimerp-accounting-ar-stock-issue-sync</c>.
/// </summary>
[Workflow("StockIssueSyncWorkflow")]
public interface IStockIssueSyncWorkflow
{
    [WorkflowRun]
    Task RunAsync(StockIssueSyncRequest request);
}
