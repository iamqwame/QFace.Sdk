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
    ///
    /// Takes a <see cref="RecordStepApprovalRequest"/> (not bare args) so the Temporal
    /// tenant-seeding interceptor — which extracts a <c>TenantId</c> property off the
    /// activity's argument object via reflection — can seed <c>ITenantContext</c> before
    /// this runs. Without it, the tenant-scoped lookup of the Workflow row by id silently
    /// finds nothing (fail-closed tenant isolation) and this activity no-ops: the signal
    /// lands and Temporal advances internally, but the DB projection never updates —
    /// "click Approve, nothing happens."
    /// </summary>
    [Activity]
    Task RecordStepApprovalAsync(RecordStepApprovalRequest request);

    /// <summary>
    /// Marks the Platform Workflow record as Approved and sets completion fields.
    /// Called after FinalizeApprovalAsync on the last step to confirm completion.
    /// </summary>
    [Activity]
    Task CompleteWorkflowRecordAsync(CompleteWorkflowRecordRequest request);

    /// <summary>Marks the Platform Workflow record as Rejected.</summary>
    [Activity]
    Task RecordRejectionAsync(RecordRejectionRequest request);

    /// <summary>
    /// Marks the Platform Workflow record as Cancelled (timed out).
    /// </summary>
    [Activity]
    Task RecordTimeoutAsync(RecordTimeoutRequest request);
}

/// <summary>
/// Argument objects for <see cref="IWorkflowPlatformActivity"/> — each carries a public
/// <c>TenantId</c> property for the Temporal tenant-seeding interceptor (see interface
/// doc comment on <see cref="IWorkflowPlatformActivity.RecordStepApprovalAsync"/>).
/// </summary>
public record RecordStepApprovalRequest(
    Guid PlatformRecordId,
    WorkflowStep Step,
    ApprovalSignal Signal,
    bool IsLastStep,
    string? NextStepCode,
    string TenantId);

public record CompleteWorkflowRecordRequest(Guid PlatformRecordId, string TenantId);

public record RecordRejectionRequest(
    Guid PlatformRecordId,
    WorkflowStep Step,
    ApprovalSignal Signal,
    string TenantId);

public record RecordTimeoutRequest(Guid PlatformRecordId, WorkflowStep Step, string TenantId);
