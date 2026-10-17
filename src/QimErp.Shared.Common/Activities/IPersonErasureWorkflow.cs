using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities;

/// <summary>
/// Temporal workflow interface for person erasure fan-out (DATA-23).
/// Implemented in a future beachhead; fans out module pseudonymise activities.
/// Task queue: qimerp-corehr-person-erasure
/// </summary>
[Workflow("PersonErasureWorkflow")]
public interface IPersonErasureWorkflow
{
    [WorkflowRun]
    Task RunAsync(PersonErasureRequest request);
}
