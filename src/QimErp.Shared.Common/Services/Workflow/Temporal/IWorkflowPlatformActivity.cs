namespace QimErp.Shared.Common.Services.Workflow.Temporal;

/// <summary>
/// Activity interface for platform-level workflow operations (Platform Workflow DB).
/// Implemented by WorkflowPlatformActivity in QimErp.Platform.Workflow.Worker.
/// </summary>
public interface IWorkflowPlatformActivity
{
    /// <summary>
    /// Fetches the WorkflowDefinition and creates the Workflow record in the Platform DB.
    /// Replaces: WorkflowCoreConsumer.ProcessWorkflowApprovalRequired
    /// Returns WorkflowInitResult (definition + DB record Guid) so the workflow loop
    /// can iterate steps and subsequent activities can reference the correct record.
    /// </summary>
    Task<WorkflowInitResult> InitializeWorkflowRecordAsync(ApprovalWorkflowInput input);

    /// <summary>
    /// Records a single step approval in the Platform DB (WorkflowHistoryEntry)
    /// and advances CurrentState / NextStep on the workflow record.
    /// Replaces: WorkflowCoreConsumer.ProcessWorkflowApprovalProcessed (step transition path)
    /// </summary>
    /// <param name="nextStepCode">
    /// The step code the workflow moves to after this approval.
    /// Null when isLastStep is true.
    /// </param>
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
    Task CompleteWorkflowRecordAsync(Guid platformRecordId);

    /// <summary>
    /// Marks the Platform Workflow record as Rejected.
    /// Replaces: WorkflowCoreConsumer processing the WorkflowRejectionRequestEvent
    /// </summary>
    Task RecordRejectionAsync(
        Guid platformRecordId,
        WorkflowStep step,
        ApprovalSignal signal);

    /// <summary>
    /// Marks the Platform Workflow record as Cancelled (timed out).
    /// </summary>
    Task RecordTimeoutAsync(Guid platformRecordId, WorkflowStep step);
}
