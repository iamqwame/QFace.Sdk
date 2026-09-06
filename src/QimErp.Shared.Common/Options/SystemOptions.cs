namespace QimErp.Shared.Common.Options;

/// <summary>
/// System identity configuration for default/system user context.
/// Bind from "System" section. Env: System__DefaultSystemEmail, etc.
/// </summary>
public class SystemOptions
{
    public const string SectionName = "System";
    public string DefaultUserId { get; set; } = "system";
    public string DefaultUserName { get; set; } = "System";
    public string DefaultSystemEmail { get; set; } = "system@qimerp.com";
    public string ConsumerSystemEmail { get; set; } = "system@consumer";
    public string DefaultNextStepName { get; set; } = "Final Review";
    public string DefaultRequesterName { get; set; } = "Requester";
    public string DefaultApproverName { get; set; } = "Approver";
    public string DefaultWorkflowCodeDisplayName { get; set; } = "Workflow Request";

    /// <summary>
    /// When true, Temporal NotificationActivity owns step emails; legacy processor skips duplicate sends.
    /// </summary>
    public bool TemporalOwnsWorkflowNotifications { get; set; } = true;

    public CompanyOptions Company { get; set; } = new();
}

/// <summary>
/// Bind from "System:Company". Env: System__Company__ForceInactive.
/// </summary>
public class CompanyOptions
{
    /// <summary>
    /// Config-level kill switch. When true every request resolves to <c>CompanyScope.Inactive</c>
    /// regardless of JWT claims or headers.
    /// </summary>
    public bool ForceInactive { get; set; }
}
