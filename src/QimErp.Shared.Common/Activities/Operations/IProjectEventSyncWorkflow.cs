using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities.Operations;

/// <summary>
/// Temporal workflow that fans out project lifecycle events to module task queues.
/// Implemented by ProjectEventSyncWorkflow in QimErp.Operations.Project.WebApi.
/// Task queue: qimerp-operations-project-event-sync
/// </summary>
[Workflow("ProjectEventSyncWorkflow")]
public interface IProjectEventSyncWorkflow
{
    [WorkflowRun]
    Task RunAsync(ProjectEventSyncRequest request);
}
