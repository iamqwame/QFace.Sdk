using Temporalio.Workflows;

namespace QimErp.Shared.Common.Services.Workflow.Temporal;

/// <summary>
/// Temporal workflow interface.
/// Implemented by ApprovalWorkflow in QimErp.Platform.Workflow.Worker.
/// Referenced here so the trigger bridge, module signal endpoints, and the Worker
/// share the same contract without cross-project dependencies.
///
/// The [Workflow] / [WorkflowRun] / [WorkflowSignal] / [WorkflowQuery] attributes
/// must live on the interface so callers that pass <c>IApprovalWorkflow</c> as
/// the generic argument to <c>TemporalClient.StartWorkflowAsync</c> can have the
/// SDK resolve the run method without a cross-project dependency on the concrete
/// workflow class. The concrete class also declares the matching attributes for
/// the Worker's own discovery path.
/// </summary>
[Workflow("ApprovalWorkflow")]
public interface IApprovalWorkflow
{
    [WorkflowRun]
    Task RunAsync(ApprovalWorkflowInput input);

    /// <summary>Called by ApproveWorkflow endpoint to advance the workflow.</summary>
    [WorkflowSignal]
    Task ApproveStepAsync(ApprovalSignal signal);

    /// <summary>Called by RejectWorkflow endpoint to terminate the workflow with rejection.</summary>
    [WorkflowSignal]
    Task RejectStepAsync(ApprovalSignal signal);

    /// <summary>UI query — returns the current step code without a DB round-trip.</summary>
    [WorkflowQuery]
    string GetCurrentState();
}
