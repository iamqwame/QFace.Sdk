using Temporalio.Workflows;

namespace QimErp.Shared.Common.Services.Workflow.Temporal;

/// <summary>
/// Temporal workflow interface.
/// Implemented by ApprovalWorkflow in QimErp.Platform.Workflow.Worker.
/// Referenced here so the trigger bridge, module signal endpoints, and the Worker
/// share the same contract without cross-project dependencies.
///
/// The [Workflow] / [WorkflowRun] attributes must live on the interface so callers that
/// pass <c>IApprovalWorkflow</c> as the generic argument to <c>TemporalClient.StartWorkflowAsync</c>
/// can resolve the run method without a cross-project dependency on the concrete workflow class.
///
/// [WorkflowSignal] and [WorkflowQuery] must live on the concrete <c>ApprovalWorkflow</c> class
/// only (see TenantSetupWorkflow). Duplicating them here prevents the worker from invoking
/// the class signal handlers during replay.
/// </summary>
[Workflow("ApprovalWorkflow")]
public interface IApprovalWorkflow
{
    [WorkflowRun]
    Task RunAsync(ApprovalWorkflowInput input);

    /// <summary>Called by ApproveWorkflow endpoint to advance the workflow.</summary>
    Task ApproveStepAsync(ApprovalSignal signal);

    /// <summary>Called by RejectWorkflow endpoint to terminate the workflow with rejection.</summary>
    Task RejectStepAsync(ApprovalSignal signal);

    /// <summary>Called by ReturnWorkflow endpoint to send the request back to the submitter for edits.</summary>
    Task ReturnStepAsync(ApprovalSignal signal);

    /// <summary>UI query — returns the current step code without a DB round-trip.</summary>
    string GetCurrentState();
}
