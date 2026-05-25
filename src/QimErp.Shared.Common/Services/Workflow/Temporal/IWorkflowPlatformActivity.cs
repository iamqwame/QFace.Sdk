using Temporalio.Activities;

namespace QimErp.Shared.Common.Services.Workflow.Temporal;

/// <summary>
/// Platform-level workflow persistence (Platform Workflow DB).
/// Implemented by <c>WorkflowPlatformActivity</c> in QimErp.Platform.Workflow.WebApi.
///
/// The [Activity] attributes live on the interface so Temporal can resolve the
/// activity method when callers (e.g. ApprovalWorkflow.RunAsync) reference the
/// activity through this contract without a project dependency on the concrete
/// activity class.
/// </summary>
public interface IWorkflowPlatformActivity
{
    /// <summary>
    /// Loads the WorkflowDefinition and creates the Workflow record in the Platform DB.
    /// Returns <see cref="WorkflowInitResult"/> so the workflow can iterate steps without extra DB reads.
    /// </summary>
    [Activity]
    Task<WorkflowInitResult> InitializeWorkflowRecordAsync(ApprovalWorkflowInput input);

    /// <summary>
    /// Records a single step approval (history + CurrentState / NextStep).
    /// </summary>
    /// <param name="nextStepCode">
    /// The step code the workflow moves to after this approval.
    /// Null when isLastStep is true.
    /// </param>
    [Activity]
    Task RecordStepApprovalAsync(
        Guid platformRecordId,
        WorkflowStep step,
        ApprovalSignal signal,
        bool isLastStep,
        string? nextStepCode);

    /// <summary>
    /// Marks the Platform Workflow record as Approved and sets completion fields.
    /// Called after FinalizeApprovalAsync on the last step to confirm completion.
    /// </summary>
    [Activity]
    Task CompleteWorkflowRecordAsync(Guid platformRecordId);

    /// <summary>Marks the Platform Workflow record as Rejected.</summary>
    [Activity]
    Task RecordRejectionAsync(
        Guid platformRecordId,
        WorkflowStep step,
        ApprovalSignal signal);

    /// <summary>
    /// Marks the Platform Workflow record as Cancelled (timed out).
    /// </summary>
    [Activity]
    Task RecordTimeoutAsync(Guid platformRecordId, WorkflowStep step);
}
