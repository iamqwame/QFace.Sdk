namespace QimErp.Shared.Common.Services.Workflow.Temporal;

/// <summary>
/// Signal payload sent by ApproveWorkflow / RejectWorkflow endpoints into the running Temporal workflow.
/// </summary>
public class ApprovalSignal
{
    public bool IsApproved { get; set; }
    public string StepCode { get; set; } = "";
    public string ApprovedBy { get; set; } = "";
    public string? ApprovedByName { get; set; }
    /// <summary>
    /// The approver's CoreHr Employee id — NOT the IAM identity/user id. Every consumer
    /// (WorkflowPlatformActivity's ActionByEmployeeId/CompletedByEmployeeId) looks this up
    /// via IWorkflowEmployeeContextService.GetByEmployeeIdAsync, which is keyed by Employee
    /// id. Populate from ICurrentUserService.GetEmployeeId() (or the already-resolved
    /// WorkflowApproverValidationContext.Approver.EmployeeId), never GetUserId().
    /// </summary>
    public string? ApprovedById { get; set; }
    /// <summary>Approver profile picture URL for timeline avatars.</summary>
    public string? ApprovedByImage { get; set; }
    public string Comments { get; set; } = "";
    public DateTime ActedAt { get; set; } = DateTime.UtcNow;
}
