namespace QimErp.Shared.Common.Workflow.Entities;

/// <summary>
/// Rich approver record returned by IModuleApprovalActivity.ResolveApproversAsync.
/// Carries everything the NotificationActivity needs to send a targeted, personalised email.
/// </summary>
public class ResolvedApprover
{
    /// <summary>Employee's entity Id (Guid).</summary>
    public Guid Id { get; set; }

    /// <summary>Human-readable employee code, e.g. "EMP-0042".</summary>
    public string EmployeeCode { get; set; } = "";

    /// <summary>Full display name, e.g. "Alice Mensah".</summary>
    public string Name { get; set; } = "";

    /// <summary>Office / work email used for notifications.</summary>
    public string Email { get; set; } = "";

    /// <summary>URL of the employee's profile picture (nullable).</summary>
    public string? ProfilePictureUrl { get; set; }

    /// <summary>
    /// The approver type that matched this resolution
    /// ("role", "department", "direct_employee", "rank", "ou").
    /// </summary>
    public string ApproverType { get; set; } = "";
}
