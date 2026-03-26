using QimErp.Shared.Common.Services.Workflow;

namespace QimErp.Shared.Common.Entities;

public abstract class WorkflowEnabledEntity : AuditableEntity, IWorkflowEnabled
{
    // ── Core status ───────────────────────────────────────────────────────────

    public WorkflowStatus WorkflowStatus { get; set; } = WorkflowStatus.NotStarted;

    /// <summary>Platform DB workflow run record Id (WorkflowApplicationDbContext).</summary>
    public Guid? CurrentWorkflowHistoryId { get; set; }

    /// <summary>
    /// Temporal workflow instance Id. Used to send Approve/Reject signals to the
    /// running ApprovalWorkflow via ITemporalClient.
    /// </summary>
    public Guid? CurrentWorkflowInstanceId { get; set; }

    /// <summary>
    /// Which workflow configuration drives this entity (e.g. "EmployeeOnboarding").
    /// </summary>
    public string? WorkflowCode { get; set; }

    // ── Step tracking (maintained by old processors for non-Temporal modules) ─
    //
    // NOTE: For modules on the Temporal path (CoreHR, Platform), the authoritative
    // current step lives in ApprovalWorkflow._currentState (Temporal event history).
    // These fields are still written by WorkflowApprovalProcessor /
    // WorkflowRejectionProcessor for HROperations modules that have not yet migrated
    // to Temporal.

    public string? CurrentWorkflowState { get; set; }

    /// <summary>
    /// Snapshot of the workflow definition set at workflow-start time.
    /// Authoritative copy lives in the Platform workflow DB (EntityWorkflowStep).
    /// Kept here as a fallback for processors on the old RabbitMQ path.
    /// </summary>
    public WorkflowDefinition WorkflowDefinition { get; set; } = new();

    // ── Audit fields ──────────────────────────────────────────────────────────

    public string? WorkflowComments { get; set; }
    public DateTime? WorkflowInitiatedAt { get; set; }

    public string? WorkflowInitiatedByEmail { get; set; }
    public string? WorkflowInitiatedByEmployeeId { get; set; }
    public string? WorkflowInitiatedByName { get; set; }

    public string? WorkflowCompletedByEmail { get; set; }
    public string? WorkflowCompletedByEmployeeId { get; set; }
    public string? WorkflowCompletedByName { get; set; }

    public string? WorkflowRejectionReason { get; set; }
    public DateTime? WorkflowCompletedAt { get; set; }

    // ── Computed ──────────────────────────────────────────────────────────────

    public virtual string EntityType => GetType().Name;
    public virtual bool IsWorkflowEnabled => !_isWorkflowDisabledForSeeding;

    public bool IsActive => WorkflowStatus is WorkflowStatus.Approved or WorkflowStatus.NotStarted ||
                            DataStatus == DataState.Active;
    public bool IsPendingApproval  => WorkflowStatus == WorkflowStatus.InProgress;
    public bool IsRejected         => WorkflowStatus == WorkflowStatus.Rejected;
    public bool IsWorkflowComplete => WorkflowStatus is WorkflowStatus.Approved or WorkflowStatus.Rejected;

    // ── Edit / delete guards (seeding-aware) ──────────────────────────────────

    public bool CanBeEdited()
    {
        if (_isWorkflowDisabledForSeeding) return true;
        return WorkflowStatus is WorkflowStatus.NotStarted or WorkflowStatus.Rejected;
    }

    public bool CanBeDeleted()
    {
        if (_isWorkflowDisabledForSeeding) return true;
        return WorkflowStatus != WorkflowStatus.Approved;
    }

    // ── Seeding helpers ───────────────────────────────────────────────────────

    private bool _isWorkflowDisabledForSeeding = false;

    public void DisableWorkflowForSeeding()
    {
        _isWorkflowDisabledForSeeding = true;
        WorkflowStatus = WorkflowStatus.NotStarted;
    }

    public void EnableWorkflowAfterSeeding()
    {
        _isWorkflowDisabledForSeeding = false;
        WorkflowStatus = WorkflowStatus.Approved;
        WorkflowCompletedAt = DateTime.UtcNow;
        WorkflowCompletedByEmail = "system@qimerp.com";
        WorkflowCompletedByEmployeeId = "system";
        WorkflowCompletedByName = "System";
        WorkflowComments = "Auto-approved during seeding";
    }

    public void UpdateWorkflowStatusDuringSeeding(WorkflowStatus status)
    {
        WorkflowStatus = status;
        if (status == WorkflowStatus.Approved)
        {
            WorkflowCompletedAt = DateTime.UtcNow;
            WorkflowCompletedByEmail = "system@qimerp.com";
            WorkflowCompletedByEmployeeId = "system";
            WorkflowCompletedByName = "System";
            WorkflowComments = "Intermediate approval during seeding";
        }
    }
}
