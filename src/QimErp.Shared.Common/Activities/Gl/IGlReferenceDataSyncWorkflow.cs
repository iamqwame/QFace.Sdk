using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities.Gl;

/// <summary>
/// Temporal workflow that fans out GL reference data changes to module task queues.
/// Implemented by GlReferenceDataSyncWorkflow in QimErp.Accounting.Core.WebApi.
/// Task queue: qimerp-accounting-gl-reference-sync
/// </summary>
[Workflow("GlReferenceDataSyncWorkflow")]
public interface IGlReferenceDataSyncWorkflow
{
    [WorkflowRun]
    Task RunAsync(GlReferenceDataSyncRequest request);
}
