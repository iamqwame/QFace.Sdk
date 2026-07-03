using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities.Operations;

/// <summary>
/// Temporal workflow started by AP when a bill is posted to GL.
/// Fans out to Operations Project expenditure sync workers.
/// Task queue: <c>qimerp-accounting-ap-bill-sync</c>.
/// </summary>
[Workflow("ProjectExpenditureSyncWorkflow")]
public interface IProjectExpenditureSyncWorkflow
{
    [WorkflowRun]
    Task RunAsync(ProjectExpenditureSyncRequest request);
}
