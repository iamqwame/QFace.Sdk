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
        return WorkflowStatus is WorkflowStatus.NotStarted
            or WorkflowStatus.Rejected
            or WorkflowStatus.Approved;
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

    public void EnableWorkflowProcessing()
    {
        _isWorkflowDisabledForSeeding = false;
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

    // ── Domain-status sync hooks ──────────────────────────────────────────────

    /// <summary>
    /// Called by <c>WorkflowApprovalProcessor</c> once the generic <see cref="WorkflowStatus"/>
    /// fields are set to Approved. Override on entities that carry their own domain status
    /// enum (distinct from <see cref="WorkflowStatus"/>) to keep it in sync with the real
    /// workflow outcome. No-op by default.
    /// </summary>
    public virtual void OnWorkflowApproved() { }

    /// <summary>
    /// Called by <c>WorkflowRejectionProcessor</c> once the generic <see cref="WorkflowStatus"/>
    /// fields are set to Rejected. Override on entities that carry their own domain status
    /// enum (distinct from <see cref="WorkflowStatus"/>) to keep it in sync with the real
    /// workflow outcome. No-op by default.
    /// </summary>
    public virtual void OnWorkflowRejected(string? reason) { }

    /// <summary>
    /// Called by <c>WorkflowReturnProcessor</c> once the generic <see cref="WorkflowStatus"/>
    /// fields are set to Returned. Override on entities that carry their own domain status
    /// enum (distinct from <see cref="WorkflowStatus"/>) to keep it in sync with the real
    /// workflow outcome. No-op by default.
    /// </summary>
    public virtual void OnWorkflowReturned(string? reason) { }
}
