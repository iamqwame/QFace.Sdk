using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities.Inventory;

/// <summary>
/// Started by AR when a customer is created, updated, or deleted.
/// Task queue: <c>qimerp-accounting-ar-customer-sync</c>.
/// </summary>
[Workflow("CustomerSyncWorkflow")]
public interface ICustomerSyncWorkflow
{
    [WorkflowRun]
    Task RunAsync(CustomerSyncRequest request);
}
