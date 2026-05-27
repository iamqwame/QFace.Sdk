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
