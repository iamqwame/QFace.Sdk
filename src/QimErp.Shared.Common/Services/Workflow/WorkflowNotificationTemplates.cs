namespace QimErp.Shared.Common.Services.Workflow;

/// <summary>
/// S3 email template codes under <c>email-templates/</c> used by Temporal <see cref="Temporal.INotificationActivity"/>.
/// Legacy <see cref="WorkflowApprovalProcessor"/> uses PascalCase names when Temporal is disabled.
/// </summary>
public static class WorkflowNotificationTemplates
{
    public const string ApproverActionRequired = "approval-request";
    public const string WorkflowStarted = "workflow-started";
    public const string StepAdvanced = "approval-stage-advanced";
    public const string CompletionDefault = "approval-approved";
    public const string RejectionDefault = "approval-rejected";
    public const string TimeoutEscalation = "approval-reminder";
    public const string Reminder = "approval-reminder";

    public const string LeaveApproved = "leave-approved";
    public const string LeaveRejected = "leave-rejected";

    public static string ApprovedForEntity(string? entityType) =>
        IsLeaveEntity(entityType) ? LeaveApproved : CompletionDefault;

    public static string RejectedForEntity(string? entityType) =>
        IsLeaveEntity(entityType) ? LeaveRejected : RejectionDefault;

    private static bool IsLeaveEntity(string? entityType) =>
        string.Equals(entityType, "EmployeeLeaveRequest", StringComparison.OrdinalIgnoreCase)
        || string.Equals(entityType, "LeaveRequest", StringComparison.OrdinalIgnoreCase)
        || string.Equals(entityType, "SickLeave", StringComparison.OrdinalIgnoreCase)
        || string.Equals(entityType, "MaternityLeave", StringComparison.OrdinalIgnoreCase);
}
