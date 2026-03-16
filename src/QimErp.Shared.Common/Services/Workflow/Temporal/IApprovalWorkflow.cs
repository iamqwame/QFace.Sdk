namespace QimErp.Shared.Common.Services.Workflow.Temporal;

/// <summary>
/// Temporal workflow interface.
/// Implemented by ApprovalWorkflow in QimErp.Platform.Orchestration.WebApi.
/// Referenced here so the bridge and the WebApi signal call share the same contract
/// without taking a dependency on the Orchestration project.
/// </summary>
public interface IApprovalWorkflow
{
    Task RunAsync(ApprovalWorkflowInput input);

    /// <summary>Called by ApproveWorkflow endpoint to advance the workflow.</summary>
    Task ApproveStepAsync(ApprovalSignal signal);

    /// <summary>Called by RejectWorkflow endpoint to terminate the workflow with rejection.</summary>
    Task RejectStepAsync(ApprovalSignal signal);

    /// <summary>UI query — returns the current step code without a DB round-trip.</summary>
    string GetCurrentState();
}
