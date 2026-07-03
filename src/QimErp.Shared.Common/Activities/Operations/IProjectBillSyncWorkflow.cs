using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities.Operations;

/// <summary>
/// Started by Project when a project bill is generated. Fans out to AR invoice creation workers.
/// Task queue: <c>qimerp-operations-project-bill-sync</c>.
/// </summary>
[Workflow("ProjectBillSyncWorkflow")]
public interface IProjectBillSyncWorkflow
{
    [WorkflowRun]
    Task RunAsync(ProjectBillSyncRequest request);
}
