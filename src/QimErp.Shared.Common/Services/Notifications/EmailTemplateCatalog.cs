namespace QimErp.Shared.Common.Services.Notifications;

/// <summary>
/// Single source of truth for design-system email template token contracts.
/// Aligned with <c>qimerp-saas-frontend/design_system/sessions/email-templates/catalog.js</c>.
/// </summary>
public static class EmailTemplateCatalog
{
    // ── Workflow approvals ────────────────────────────────────────────────────

    public static readonly EmailTemplateDefinition WorkflowStarted = new(
        "workflow-started",
        ["ItemTitle", "ItemCode", "FirstName", "Department", "WorkflowName", "Total", "ApproverName", "NextApprover", "ExpectedDate", "Link"],
        []);

    public static readonly EmailTemplateDefinition ApprovalRequest = new(
        "approval-request",
        ["ItemTitle", "ItemCode", "FullName", "AvatarUrl", "Department", "ApproverName", "N", "Total", "Link"],
        []);

    public static readonly EmailTemplateDefinition ApprovalStageActive = new(
        "approval-stage-active",
        ["ItemTitle", "ItemCode", "FullName", "AvatarUrl", "Department", "ApproverName", "N", "Total", "PrevApprover", "PrevDate", "Link"],
        []);

    public static readonly EmailTemplateDefinition ApprovalStageAdvanced = new(
        "approval-stage-advanced",
        ["ItemTitle", "ItemCode", "FirstName", "AvatarUrl", "ApproverName", "ApproverRole", "N", "Total", "PrevApprover", "PrevDate", "Link"],
        []);

    public static readonly EmailTemplateDefinition ApprovalApproved = new(
        "approval-approved",
        ["ItemTitle", "FirstName", "AvatarUrl", "ApproverName", "Total", "Link"],
        []);

    public static readonly EmailTemplateDefinition ApprovalRejected = new(
        "approval-rejected",
        ["ItemTitle", "FirstName", "AvatarUrl", "ApproverName", "Reason", "Link"],
        []);

    public static readonly EmailTemplateDefinition ApprovalReminder = new(
        "approval-reminder",
        ["ItemTitle", "FullName", "AvatarUrl", "Department", "ApproverName", "N", "Link"],
        []);

    // ── Onboarding / offboarding ──────────────────────────────────────────────

    public static readonly EmailTemplateDefinition OnboardingWelcome = new(
        "onboarding-welcome",
        ["FirstName", "AvatarUrl", "Department", "Manager", "StartDate"],
        ["Link"]);

    public static readonly EmailTemplateDefinition OnboardingTaskAssigned = new(
        "onboarding-task-assigned",
        ["FirstName", "LastName", "FullName", "AvatarUrl", "Department", "TaskTitle", "DayN", "DueDate", "Link"],
        []);

    public static readonly EmailTemplateDefinition OnboardingTaskReminder = new(
        "onboarding-task-reminder",
        ["FullName", "AvatarUrl", "Department", "TaskTitle", "DueDate", "N", "Link"],
        []);

    public static readonly EmailTemplateDefinition OnboardingTaskNudge = new(
        "onboarding-task-nudge",
        ["FullName", "TaskTitle"],
        ["ActorName", "Department", "DueDate", "Message"]);

    public static readonly EmailTemplateDefinition OnboardingTaskEscalation = new(
        "onboarding-task-escalation",
        ["FullName", "TaskTitle"],
        ["ActorName", "Department", "DueDate", "Message"]);

    public static readonly EmailTemplateDefinition OffboardingInitiated = new(
        "offboarding-initiated",
        ["FullName", "AvatarUrl", "Department", "LastDay", "Link"],
        []);

    public static readonly EmailTemplateDefinition OffboardingTaskAssigned = new(
        "offboarding-task-assigned",
        ["FullName", "AvatarUrl", "Department", "TaskTitle", "LastDay", "DueDate", "Link"],
        []);

    public static readonly EmailTemplateDefinition OffboardingExitInterview = new(
        "offboarding-exit-interview",
        ["FirstName", "AvatarUrl", "Link"],
        []);

    // ── Payroll & leave ─────────────────────────────────────────────────────

    public static readonly EmailTemplateDefinition PayrollPayslip = new(
        "payroll-payslip",
        ["FullName", "FirstName", "AvatarUrl", "Department", "Period", "NetPay", "GrossPay", "Deductions", "Link"],
        []);

    public static readonly EmailTemplateDefinition LeaveApproved = new(
        "leave-approved",
        ["FirstName", "AvatarUrl", "Department", "LeaveType", "Days", "FromDate", "ToDate", "Manager", "Link"],
        []);

    public static readonly EmailTemplateDefinition LeaveRejected = new(
        "leave-rejected",
        ["FirstName", "AvatarUrl", "Department", "LeaveType", "FromDate", "ToDate", "Manager", "Reason"],
        []);

    // ── Account & access ──────────────────────────────────────────────────────

    public static readonly EmailTemplateDefinition AccountWelcome = new(
        "account-welcome",
        ["FirstName", "FullName", "Email", "AvatarUrl", "Department", "Manager", "TempPassword"],
        ["Link"]);

    public static readonly EmailTemplateDefinition PasswordReset = new(
        "password-reset",
        ["Link", "IpAddress"],
        ["FirstName"]);

    public static readonly EmailTemplateDefinition PasswordResetConfirmation = new(
        "password-reset-confirmation",
        ["FirstName", "IpAddress", "LoginCity", "LoginCountry", "LoginTime"],
        []);

    public static readonly EmailTemplateDefinition LoginSuccess = new(
        "login-success",
        ["FirstName", "IpAddress", "LoginCity", "LoginCountry", "LoginTime", "Browser", "Device"],
        []);

    // ── App Store ──────────────────────────────────────────────────────────────

    public static readonly EmailTemplateDefinition AppStoreItemInstalled = new(
        "app-store-item-installed",
        ["FirstName", "ItemName", "ItemTypeLabel", "ItemInitial", "PriceLabel"],
        ["Link", "Portal", "InstalledByName"]);

    public static IReadOnlyDictionary<string, EmailTemplateDefinition> All => AllLazy.Value;

    private static readonly Lazy<IReadOnlyDictionary<string, EmailTemplateDefinition>> AllLazy = new(() =>
        new Dictionary<string, EmailTemplateDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["workflow-started"]            = WorkflowStarted,
            ["approval-request"]            = ApprovalRequest,
            ["approval-stage-active"]       = ApprovalStageActive,
            ["approval-stage-advanced"]     = ApprovalStageAdvanced,
            ["approval-approved"]           = ApprovalApproved,
            ["approval-rejected"]           = ApprovalRejected,
            ["approval-reminder"]           = ApprovalReminder,
            ["onboarding-welcome"]          = OnboardingWelcome,
            ["onboarding-task-assigned"]    = OnboardingTaskAssigned,
            ["onboarding-task-reminder"]    = OnboardingTaskReminder,
            ["onboarding-task-nudge"]       = OnboardingTaskNudge,
            ["onboarding-task-escalation"]  = OnboardingTaskEscalation,
            ["offboarding-initiated"]       = OffboardingInitiated,
            ["offboarding-task-assigned"]   = OffboardingTaskAssigned,
            ["offboarding-exit-interview"]  = OffboardingExitInterview,
            ["payroll-payslip"]             = PayrollPayslip,
            ["leave-approved"]              = LeaveApproved,
            ["leave-rejected"]              = LeaveRejected,
            ["account-welcome"]             = AccountWelcome,
            ["password-reset"]              = PasswordReset,
            ["password-reset-confirmation"] = PasswordResetConfirmation,
            ["login-success"]               = LoginSuccess,
            ["app-store-item-installed"]    = AppStoreItemInstalled,
        });

    public static EmailTemplateDefinition? TryGet(string templateCode) =>
        All.TryGetValue(templateCode, out var def) ? def : null;

    public static EmailTemplateDefinition Get(string templateCode) =>
        TryGet(templateCode)
        ?? throw new KeyNotFoundException($"Unknown email template code: {templateCode}");
}
