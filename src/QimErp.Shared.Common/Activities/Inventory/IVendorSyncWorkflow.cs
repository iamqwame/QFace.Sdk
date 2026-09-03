using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities.Inventory;

/// <summary>
/// Started by AP when a vendor is created, updated, or deleted.
/// Task queue: <c>qimerp-accounting-ap-vendor-sync</c>.
/// </summary>
[Workflow("VendorSyncWorkflow")]
public interface IVendorSyncWorkflow
{
    [WorkflowRun]
    Task RunAsync(VendorSyncRequest request);
}
