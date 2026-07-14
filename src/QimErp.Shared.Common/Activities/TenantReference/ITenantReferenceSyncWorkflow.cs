using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities.TenantReference;

/// <summary>
/// Temporal workflow that fans out IAM tenant reference data changes to module task queues.
/// Implemented by TenantReferenceSyncWorkflow in QimErp.IAM.Core.WebApi.
/// Task queue: qimerp-iam-tenant-reference-sync
/// </summary>
[Workflow("TenantReferenceSyncWorkflow")]
public interface ITenantReferenceSyncWorkflow
{
    [WorkflowRun]
    Task RunAsync(TenantReferenceSyncRequest request);
}
