using QimErp.Shared.Common.Services.Auth;

namespace QimErp.Shared.Common.Activities.TenantActivity;

public static class TenantActivityRequestBuilder
{
    public static RecordTenantActivityRequest ForEmployeeCreated(
        string tenantId,
        Guid employeeId,
        string employeeName,
        string? organizationalUnitName,
        ICurrentUserService currentUser) =>
        Build(
            tenantId,
            TenantActivityModules.Hr,
            HrActivityTypes.EmployeeCreated,
            BuildEmployeeJoinedSummary(employeeName, organizationalUnitName),
            "employee",
            employeeId,
            employeeName,
            currentUser,
            correlationSuffix: $"employee-created:{employeeId:N}");

    public static RecordTenantActivityRequest ForEmployeeUpdated(
        string tenantId,
        Guid employeeId,
        string employeeName,
        ICurrentUserService currentUser) =>
        Build(
            tenantId,
            TenantActivityModules.Hr,
            HrActivityTypes.EmployeeUpdated,
            $"Employee {employeeName} was updated",
            "employee",
            employeeId,
            employeeName,
            currentUser,
            correlationSuffix: $"employee-updated:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    // ── Department / Org Unit ──────────────────────────────────────────────
    public static RecordTenantActivityRequest ForDepartmentCreated(
        string tenantId, Guid unitId, string unitName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.DepartmentCreated,
            $"New department \"{unitName}\" was created",
            "department", unitId, unitName, currentUser,
            correlationSuffix: $"department-created:{unitId:N}");

    public static RecordTenantActivityRequest ForDepartmentUpdated(
        string tenantId, Guid unitId, string unitName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.DepartmentUpdated,
            $"Department \"{unitName}\" was updated",
            "department", unitId, unitName, currentUser,
            correlationSuffix: $"department-updated:{unitId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForDepartmentDeleted(
        string tenantId, Guid unitId, string unitName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.DepartmentDeleted,
            $"Department \"{unitName}\" was deleted",
            "department", unitId, unitName, currentUser,
            correlationSuffix: $"department-deleted:{unitId:N}");

    // ── Job Titles ─────────────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForJobTitleCreated(
        string tenantId, Guid jobTitleId, string jobTitleName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.JobTitleCreated,
            $"Job title \"{jobTitleName}\" was created",
            "job-title", jobTitleId, jobTitleName, currentUser,
            correlationSuffix: $"job-title-created:{jobTitleId:N}");

    public static RecordTenantActivityRequest ForJobTitleUpdated(
        string tenantId, Guid jobTitleId, string jobTitleName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.JobTitleUpdated,
            $"Job title \"{jobTitleName}\" was updated",
            "job-title", jobTitleId, jobTitleName, currentUser,
            correlationSuffix: $"job-title-updated:{jobTitleId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForJobTitleDeleted(
        string tenantId, Guid jobTitleId, string jobTitleName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.JobTitleDeleted,
            $"Job title \"{jobTitleName}\" was deleted",
            "job-title", jobTitleId, jobTitleName, currentUser,
            correlationSuffix: $"job-title-deleted:{jobTitleId:N}");

    // ── Leave ──────────────────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForLeaveRequestSubmitted(
        string tenantId, Guid leaveRequestId, string employeeName, string leaveTypeName,
        Guid actorUserId, string actorUserName) =>
        BuildManual(tenantId, TenantActivityModules.Hr, HrActivityTypes.LeaveRequestSubmitted,
            $"{employeeName} submitted a {leaveTypeName} request",
            "leave-request", leaveRequestId, employeeName, actorUserId, actorUserName,
            correlationSuffix: $"leave-submitted:{leaveRequestId:N}");

    public static RecordTenantActivityRequest ForLeaveRequestApproved(
        string tenantId, Guid leaveRequestId, string employeeName, string leaveTypeName,
        Guid actorUserId, string actorUserName) =>
        BuildManual(tenantId, TenantActivityModules.Hr, HrActivityTypes.LeaveRequestApproved,
            $"{employeeName}'s {leaveTypeName} request was approved",
            "leave-request", leaveRequestId, employeeName, actorUserId, actorUserName,
            correlationSuffix: $"leave-approved:{leaveRequestId:N}");

    public static RecordTenantActivityRequest ForLeaveRequestRejected(
        string tenantId, Guid leaveRequestId, string employeeName, string leaveTypeName,
        Guid actorUserId, string actorUserName) =>
        BuildManual(tenantId, TenantActivityModules.Hr, HrActivityTypes.LeaveRequestRejected,
            $"{employeeName}'s {leaveTypeName} request was rejected",
            "leave-request", leaveRequestId, employeeName, actorUserId, actorUserName,
            correlationSuffix: $"leave-rejected:{leaveRequestId:N}");

    public static RecordTenantActivityRequest ForLeaveRequestCancelled(
        string tenantId, Guid leaveRequestId, string employeeName, string leaveTypeName,
        Guid actorUserId, string actorUserName) =>
        BuildManual(tenantId, TenantActivityModules.Hr, HrActivityTypes.LeaveRequestCancelled,
            $"{employeeName}'s {leaveTypeName} request was cancelled",
            "leave-request", leaveRequestId, employeeName, actorUserId, actorUserName,
            correlationSuffix: $"leave-cancelled:{leaveRequestId:N}");

    // ── Offboarding ────────────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForOffboardingInitiated(
        string tenantId, Guid instanceId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.OffboardingInitiated,
            $"Offboarding initiated for {employeeName}",
            "offboarding", instanceId, employeeName, currentUser,
            correlationSuffix: $"offboarding-initiated:{instanceId:N}");

    public static RecordTenantActivityRequest ForOffboardingApproved(
        string tenantId, Guid instanceId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.OffboardingApproved,
            $"Offboarding approved for {employeeName}",
            "offboarding", instanceId, employeeName, currentUser,
            correlationSuffix: $"offboarding-approved:{instanceId:N}");

    public static RecordTenantActivityRequest ForOffboardingCancelled(
        string tenantId, Guid instanceId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.OffboardingCancelled,
            $"Offboarding cancelled for {employeeName}",
            "offboarding", instanceId, employeeName, currentUser,
            correlationSuffix: $"offboarding-cancelled:{instanceId:N}");

    public static RecordTenantActivityRequest ForOffboardingCompleted(
        string tenantId, Guid instanceId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.OffboardingCompleted,
            $"Offboarding completed for {employeeName}",
            "offboarding", instanceId, employeeName, currentUser,
            correlationSuffix: $"offboarding-completed:{instanceId:N}");

    // ── Employee assignments ───────────────────────────────────────────────
    public static RecordTenantActivityRequest ForEmployeeJobTitleChanged(
        string tenantId, Guid employeeId, string employeeName, string jobTitleName,
        ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.EmployeeJobTitleChanged,
            $"{employeeName} was assigned job title \"{jobTitleName}\"",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"job-title-changed:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForEmployeeOrgUnitChanged(
        string tenantId, Guid employeeId, string employeeName, string orgUnitName,
        ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.EmployeeOrgUnitChanged,
            $"{employeeName} was moved to \"{orgUnitName}\"",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"org-unit-changed:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    // ── Payroll (grade bulk) ───────────────────────────────────────────────
    public static RecordTenantActivityRequest ForGradeBulkAssigned(
        string tenantId, string gradeCode, int count, Guid actorUserId, string actorUserName) =>
        BuildManual(tenantId, TenantActivityModules.Hr, PayrollActivityTypes.GradeBulkAssigned,
            $"Grade {gradeCode} assigned to {count} employee{(count == 1 ? "" : "s")}",
            "grade", Guid.Empty, gradeCode, actorUserId, actorUserName,
            correlationSuffix: $"grade-bulk-assigned:{gradeCode}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForEmployeeDeactivated(
        string tenantId,
        Guid employeeId,
        string employeeName,
        ICurrentUserService currentUser) =>
        Build(
            tenantId,
            TenantActivityModules.Hr,
            HrActivityTypes.EmployeeDeactivated,
            $"Employee {employeeName} was deactivated",
            "employee",
            employeeId,
            employeeName,
            currentUser,
            correlationSuffix: $"employee-deactivated:{employeeId:N}");

    // ── Employee Lifecycle ─────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForEmployeeActivated(
        string tenantId, Guid employeeId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.EmployeeActivated,
            $"Employee {name} was activated",
            "employee", employeeId, name, currentUser,
            correlationSuffix: $"employee-activated:{employeeId:N}");

    // ── Rank ───────────────────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForRankCreated(
        string tenantId, Guid id, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.RankCreated,
            $"Rank \"{name}\" was created",
            "rank", id, name, currentUser,
            correlationSuffix: $"rank-created:{id:N}");

    public static RecordTenantActivityRequest ForRankUpdated(
        string tenantId, Guid id, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.RankUpdated,
            $"Rank \"{name}\" was updated",
            "rank", id, name, currentUser,
            correlationSuffix: $"rank-updated:{id:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForRankDeleted(
        string tenantId, Guid id, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.RankDeleted,
            $"Rank \"{name}\" was deleted",
            "rank", id, name, currentUser,
            correlationSuffix: $"rank-deleted:{id:N}");

    // ── Station ────────────────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForStationCreated(
        string tenantId, Guid id, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.StationCreated,
            $"Station \"{name}\" was created",
            "station", id, name, currentUser,
            correlationSuffix: $"station-created:{id:N}");

    public static RecordTenantActivityRequest ForStationUpdated(
        string tenantId, Guid id, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.StationUpdated,
            $"Station \"{name}\" was updated",
            "station", id, name, currentUser,
            correlationSuffix: $"station-updated:{id:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForStationDeleted(
        string tenantId, Guid id, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.StationDeleted,
            $"Station \"{name}\" was deleted",
            "station", id, name, currentUser,
            correlationSuffix: $"station-deleted:{id:N}");

    // ── Job Status ─────────────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForJobStatusCreated(
        string tenantId, Guid id, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.JobStatusCreated,
            $"Job status \"{name}\" was created",
            "job-status", id, name, currentUser,
            correlationSuffix: $"job-status-created:{id:N}");

    public static RecordTenantActivityRequest ForJobStatusUpdated(
        string tenantId, Guid id, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.JobStatusUpdated,
            $"Job status \"{name}\" was updated",
            "job-status", id, name, currentUser,
            correlationSuffix: $"job-status-updated:{id:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForJobStatusDeleted(
        string tenantId, Guid id, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.JobStatusDeleted,
            $"Job status \"{name}\" was deleted",
            "job-status", id, name, currentUser,
            correlationSuffix: $"job-status-deleted:{id:N}");

    // ── Onboarding ─────────────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForOnboardingStarted(
        string tenantId, Guid employeeId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrOnboardingActivityTypes.OnboardingStarted,
            $"Onboarding started for {employeeName}",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"onboarding-started:{employeeId:N}");

    public static RecordTenantActivityRequest ForOnboardingCancelled(
        string tenantId, Guid employeeId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrOnboardingActivityTypes.OnboardingCancelled,
            $"Onboarding cancelled for {employeeName}",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"onboarding-cancelled:{employeeId:N}");

    public static RecordTenantActivityRequest ForOnboardingMetaUpdated(
        string tenantId, Guid employeeId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrOnboardingActivityTypes.OnboardingMetaUpdated,
            $"Onboarding details updated for {employeeName}",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"onboarding-meta-updated:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForOnboardingTaskCompleted(
        string tenantId, Guid taskId, string taskTitle, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrOnboardingActivityTypes.OnboardingTaskCompleted,
            $"Onboarding task \"{taskTitle}\" was completed",
            "onboarding-task", taskId, taskTitle, currentUser,
            correlationSuffix: $"onboarding-task-completed:{taskId:N}");

    public static RecordTenantActivityRequest ForOnboardingTaskReopened(
        string tenantId, Guid taskId, string taskTitle, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrOnboardingActivityTypes.OnboardingTaskReopened,
            $"Onboarding task \"{taskTitle}\" was reopened",
            "onboarding-task", taskId, taskTitle, currentUser,
            correlationSuffix: $"onboarding-task-reopened:{taskId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForOnboardingTaskSkipped(
        string tenantId, Guid taskId, string taskTitle, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrOnboardingActivityTypes.OnboardingTaskSkipped,
            $"Onboarding task \"{taskTitle}\" was skipped",
            "onboarding-task", taskId, taskTitle, currentUser,
            correlationSuffix: $"onboarding-task-skipped:{taskId:N}");

    public static RecordTenantActivityRequest ForOnboardingTaskReassigned(
        string tenantId, Guid taskId, string taskTitle, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrOnboardingActivityTypes.OnboardingTaskReassigned,
            $"Onboarding task \"{taskTitle}\" was reassigned",
            "onboarding-task", taskId, taskTitle, currentUser,
            correlationSuffix: $"onboarding-task-reassigned:{taskId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForOnboardingTaskEscalated(
        string tenantId, Guid taskId, string taskTitle, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrOnboardingActivityTypes.OnboardingTaskEscalated,
            $"Onboarding task \"{taskTitle}\" was escalated",
            "onboarding-task", taskId, taskTitle, currentUser,
            correlationSuffix: $"onboarding-task-escalated:{taskId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForOnboardingAdhocTaskAdded(
        string tenantId, Guid taskId, string taskTitle, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrOnboardingActivityTypes.OnboardingAdhocTaskAdded,
            $"Ad-hoc onboarding task \"{taskTitle}\" was added",
            "onboarding-task", taskId, taskTitle, currentUser,
            correlationSuffix: $"onboarding-adhoc-task-added:{taskId:N}");

    public static RecordTenantActivityRequest ForOnboardingTemplateCreated(
        string tenantId, Guid templateId, string templateName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrOnboardingActivityTypes.OnboardingTemplateCreated,
            $"Onboarding template \"{templateName}\" was created",
            "onboarding-template", templateId, templateName, currentUser,
            correlationSuffix: $"onboarding-template-created:{templateId:N}");

    public static RecordTenantActivityRequest ForOnboardingTemplateUpdated(
        string tenantId, Guid templateId, string templateName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrOnboardingActivityTypes.OnboardingTemplateUpdated,
            $"Onboarding template \"{templateName}\" was updated",
            "onboarding-template", templateId, templateName, currentUser,
            correlationSuffix: $"onboarding-template-updated:{templateId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForOnboardingTemplateDeleted(
        string tenantId, Guid templateId, string templateName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrOnboardingActivityTypes.OnboardingTemplateDeleted,
            $"Onboarding template \"{templateName}\" was deleted",
            "onboarding-template", templateId, templateName, currentUser,
            correlationSuffix: $"onboarding-template-deleted:{templateId:N}");

    public static RecordTenantActivityRequest ForOnboardingTemplateDefaultSet(
        string tenantId, Guid templateId, string templateName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrOnboardingActivityTypes.OnboardingTemplateDefaultSet,
            $"Onboarding template \"{templateName}\" was set as default",
            "onboarding-template", templateId, templateName, currentUser,
            correlationSuffix: $"onboarding-template-default-set:{templateId:N}");

    // ── Performance ────────────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForPerformanceReviewCreated(
        string tenantId, Guid reviewId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.ReviewCreated,
            summary, "performance-review", reviewId, summary, currentUser,
            correlationSuffix: $"review-created:{reviewId:N}");

    public static RecordTenantActivityRequest ForSelfReviewSubmitted(
        string tenantId, Guid reviewId, string summary, Guid actorUserId, string actorUserName) =>
        BuildManual(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.SelfReviewSubmitted,
            summary, "performance-review", reviewId, summary, actorUserId, actorUserName,
            correlationSuffix: $"self-review-submitted:{reviewId:N}");

    public static RecordTenantActivityRequest ForManagerReviewSubmitted(
        string tenantId, Guid reviewId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.ManagerReviewSubmitted,
            summary, "performance-review", reviewId, summary, currentUser,
            correlationSuffix: $"manager-review-submitted:{reviewId:N}");

    public static RecordTenantActivityRequest ForHRReviewSubmitted(
        string tenantId, Guid reviewId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.HRReviewSubmitted,
            summary, "performance-review", reviewId, summary, currentUser,
            correlationSuffix: $"hr-review-submitted:{reviewId:N}");

    public static RecordTenantActivityRequest ForPerformanceReviewFinalized(
        string tenantId, Guid reviewId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.ReviewFinalized,
            summary, "performance-review", reviewId, summary, currentUser,
            correlationSuffix: $"review-finalized:{reviewId:N}");

    public static RecordTenantActivityRequest ForPerformanceReviewAcknowledged(
        string tenantId, Guid reviewId, string summary, Guid actorUserId, string actorUserName) =>
        BuildManual(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.ReviewAcknowledged,
            summary, "performance-review", reviewId, summary, actorUserId, actorUserName,
            correlationSuffix: $"review-acknowledged:{reviewId:N}");

    public static RecordTenantActivityRequest ForGoalCreated(
        string tenantId, Guid goalId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.GoalCreated,
            $"Goal \"{title}\" was created",
            "goal", goalId, title, currentUser,
            correlationSuffix: $"goal-created:{goalId:N}");

    public static RecordTenantActivityRequest ForGoalUpdated(
        string tenantId, Guid goalId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.GoalUpdated,
            $"Goal \"{title}\" was updated",
            "goal", goalId, title, currentUser,
            correlationSuffix: $"goal-updated:{goalId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForGoalProgressUpdated(
        string tenantId, Guid goalId, string title, Guid actorUserId, string actorUserName) =>
        BuildManual(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.GoalProgressUpdated,
            $"Progress updated on goal \"{title}\"",
            "goal", goalId, title, actorUserId, actorUserName,
            correlationSuffix: $"goal-progress-updated:{goalId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForGoalApproved(
        string tenantId, Guid goalId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.GoalApproved,
            $"Goal \"{title}\" was approved",
            "goal", goalId, title, currentUser,
            correlationSuffix: $"goal-approved:{goalId:N}");

    public static RecordTenantActivityRequest ForGoalCompleted(
        string tenantId, Guid goalId, string title, Guid actorUserId, string actorUserName) =>
        BuildManual(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.GoalCompleted,
            $"Goal \"{title}\" was completed",
            "goal", goalId, title, actorUserId, actorUserName,
            correlationSuffix: $"goal-completed:{goalId:N}");

    public static RecordTenantActivityRequest ForGoalAligned(
        string tenantId, Guid goalId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.GoalAligned,
            $"Goal \"{title}\" was aligned",
            "goal", goalId, title, currentUser,
            correlationSuffix: $"goal-aligned:{goalId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForGoalEvidenceUploaded(
        string tenantId, Guid goalId, string title, Guid actorUserId, string actorUserName) =>
        BuildManual(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.GoalEvidenceUploaded,
            $"Evidence uploaded for goal \"{title}\"",
            "goal", goalId, title, actorUserId, actorUserName,
            correlationSuffix: $"goal-evidence-uploaded:{goalId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForCheckInCreated(
        string tenantId, Guid checkInId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.CheckInCreated,
            summary, "check-in", checkInId, summary, currentUser,
            correlationSuffix: $"check-in-created:{checkInId:N}");

    public static RecordTenantActivityRequest ForCheckInUpdated(
        string tenantId, Guid checkInId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.CheckInUpdated,
            summary, "check-in", checkInId, summary, currentUser,
            correlationSuffix: $"check-in-updated:{checkInId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForCheckInManagerFeedbackAdded(
        string tenantId, Guid checkInId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.CheckInManagerFeedbackAdded,
            summary, "check-in", checkInId, summary, currentUser,
            correlationSuffix: $"check-in-manager-feedback-added:{checkInId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForCalibrationCreated(
        string tenantId, Guid calibrationId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.CalibrationCreated,
            summary, "calibration", calibrationId, summary, currentUser,
            correlationSuffix: $"calibration-created:{calibrationId:N}");

    public static RecordTenantActivityRequest ForCalibrationRatingAdjusted(
        string tenantId, Guid calibrationId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.CalibrationRatingAdjusted,
            summary, "calibration", calibrationId, summary, currentUser,
            correlationSuffix: $"calibration-rating-adjusted:{calibrationId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForCalibrationCompleted(
        string tenantId, Guid calibrationId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.CalibrationCompleted,
            summary, "calibration", calibrationId, summary, currentUser,
            correlationSuffix: $"calibration-completed:{calibrationId:N}");

    public static RecordTenantActivityRequest ForFeedback360Created(
        string tenantId, Guid feedbackId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.Feedback360Created,
            summary, "feedback360", feedbackId, summary, currentUser,
            correlationSuffix: $"feedback360-created:{feedbackId:N}");

    public static RecordTenantActivityRequest ForFeedback360ProviderAdded(
        string tenantId, Guid feedbackId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.Feedback360ProviderAdded,
            summary, "feedback360", feedbackId, summary, currentUser,
            correlationSuffix: $"feedback360-provider-added:{feedbackId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForFeedback360Completed(
        string tenantId, Guid feedbackId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.Feedback360Completed,
            summary, "feedback360", feedbackId, summary, currentUser,
            correlationSuffix: $"feedback360-completed:{feedbackId:N}");

    public static RecordTenantActivityRequest ForDevelopmentPlanCreated(
        string tenantId, Guid planId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.DevelopmentPlanCreated,
            $"Development plan \"{title}\" was created",
            "development-plan", planId, title, currentUser,
            correlationSuffix: $"development-plan-created:{planId:N}");

    public static RecordTenantActivityRequest ForDevelopmentPlanActivityAdded(
        string tenantId, Guid planId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.DevelopmentPlanActivityAdded,
            $"Activity added to development plan \"{title}\"",
            "development-plan", planId, title, currentUser,
            correlationSuffix: $"development-plan-activity-added:{planId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForDevelopmentPlanActivityCompleted(
        string tenantId, Guid planId, string title, Guid actorUserId, string actorUserName) =>
        BuildManual(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.DevelopmentPlanActivityCompleted,
            $"Activity completed in development plan \"{title}\"",
            "development-plan", planId, title, actorUserId, actorUserName,
            correlationSuffix: $"development-plan-activity-completed:{planId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForDevelopmentPlanApproved(
        string tenantId, Guid planId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.DevelopmentPlanApproved,
            $"Development plan \"{title}\" was approved",
            "development-plan", planId, title, currentUser,
            correlationSuffix: $"development-plan-approved:{planId:N}");

    public static RecordTenantActivityRequest ForDevelopmentPlanCompleted(
        string tenantId, Guid planId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.DevelopmentPlanCompleted,
            $"Development plan \"{title}\" was completed",
            "development-plan", planId, title, currentUser,
            correlationSuffix: $"development-plan-completed:{planId:N}");

    // ── Talent ─────────────────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForTalentReviewCreated(
        string tenantId, Guid reviewId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.TalentReviewCreated,
            summary, "talent-review", reviewId, summary, currentUser,
            correlationSuffix: $"talent-review-created:{reviewId:N}");

    public static RecordTenantActivityRequest ForTalentManagerReviewSubmitted(
        string tenantId, Guid reviewId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.TalentManagerReviewSubmitted,
            summary, "talent-review", reviewId, summary, currentUser,
            correlationSuffix: $"talent-manager-review-submitted:{reviewId:N}");

    public static RecordTenantActivityRequest ForTalentHRReviewSubmitted(
        string tenantId, Guid reviewId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.TalentHRReviewSubmitted,
            summary, "talent-review", reviewId, summary, currentUser,
            correlationSuffix: $"talent-hr-review-submitted:{reviewId:N}");

    public static RecordTenantActivityRequest ForTalentReviewLinkedToPerformance(
        string tenantId, Guid reviewId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.TalentReviewLinkedToPerformance,
            summary, "talent-review", reviewId, summary, currentUser,
            correlationSuffix: $"talent-review-linked-to-performance:{reviewId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTalentReviewUpdated(
        string tenantId, Guid reviewId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.TalentReviewUpdated,
            summary, "talent-review", reviewId, summary, currentUser,
            correlationSuffix: $"talent-review-updated:{reviewId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTalentReviewCompleted(
        string tenantId, Guid reviewId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.TalentReviewCompleted,
            summary, "talent-review", reviewId, summary, currentUser,
            correlationSuffix: $"talent-review-completed:{reviewId:N}");

    public static RecordTenantActivityRequest ForSuccessionPlanCreated(
        string tenantId, Guid planId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.SuccessionPlanCreated,
            $"Succession plan \"{title}\" was created",
            "succession-plan", planId, title, currentUser,
            correlationSuffix: $"succession-plan-created:{planId:N}");

    public static RecordTenantActivityRequest ForSuccessionPlanUpdated(
        string tenantId, Guid planId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.SuccessionPlanUpdated,
            $"Succession plan \"{title}\" was updated",
            "succession-plan", planId, title, currentUser,
            correlationSuffix: $"succession-plan-updated:{planId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForSuccessionPlanDeleted(
        string tenantId, Guid planId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.SuccessionPlanDeleted,
            $"Succession plan \"{title}\" was deleted",
            "succession-plan", planId, title, currentUser,
            correlationSuffix: $"succession-plan-deleted:{planId:N}");

    public static RecordTenantActivityRequest ForSuccessionPlanActivated(
        string tenantId, Guid planId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.SuccessionPlanActivated,
            $"Succession plan \"{title}\" was activated",
            "succession-plan", planId, title, currentUser,
            correlationSuffix: $"succession-plan-activated:{planId:N}");

    public static RecordTenantActivityRequest ForSuccessionPlanCompleted(
        string tenantId, Guid planId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.SuccessionPlanCompleted,
            $"Succession plan \"{title}\" was completed",
            "succession-plan", planId, title, currentUser,
            correlationSuffix: $"succession-plan-completed:{planId:N}");

    public static RecordTenantActivityRequest ForSuccessionPlanCancelled(
        string tenantId, Guid planId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.SuccessionPlanCancelled,
            $"Succession plan \"{title}\" was cancelled",
            "succession-plan", planId, title, currentUser,
            correlationSuffix: $"succession-plan-cancelled:{planId:N}");

    public static RecordTenantActivityRequest ForSuccessionCandidateAdded(
        string tenantId, Guid planId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.SuccessionCandidateAdded,
            $"{employeeName} added as succession candidate",
            "succession-plan", planId, employeeName, currentUser,
            correlationSuffix: $"succession-candidate-added:{planId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForSuccessionCandidateReadinessUpdated(
        string tenantId, Guid planId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.SuccessionCandidateReadinessUpdated,
            $"Readiness updated for succession candidate {employeeName}",
            "succession-plan", planId, employeeName, currentUser,
            correlationSuffix: $"succession-candidate-readiness-updated:{planId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForSuccessionCandidateRemoved(
        string tenantId, Guid planId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.SuccessionCandidateRemoved,
            $"{employeeName} removed from succession plan",
            "succession-plan", planId, employeeName, currentUser,
            correlationSuffix: $"succession-candidate-removed:{planId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTalentPipelineCreated(
        string tenantId, Guid pipelineId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.TalentPipelineCreated,
            $"Talent pipeline \"{title}\" was created",
            "talent-pipeline", pipelineId, title, currentUser,
            correlationSuffix: $"talent-pipeline-created:{pipelineId:N}");

    public static RecordTenantActivityRequest ForTalentPipelineUpdated(
        string tenantId, Guid pipelineId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.TalentPipelineUpdated,
            $"Talent pipeline \"{title}\" was updated",
            "talent-pipeline", pipelineId, title, currentUser,
            correlationSuffix: $"talent-pipeline-updated:{pipelineId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTalentPipelineStageUpdated(
        string tenantId, Guid pipelineId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.TalentPipelineStageUpdated,
            $"Stage updated in talent pipeline \"{title}\"",
            "talent-pipeline", pipelineId, title, currentUser,
            correlationSuffix: $"talent-pipeline-stage-updated:{pipelineId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTalentNineBoxPositionUpdated(
        string tenantId, Guid employeeId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.TalentNineBoxPositionUpdated,
            $"Nine-box position updated for {employeeName}",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"talent-nine-box-position-updated:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTalentEmployeeMarkedHighPotential(
        string tenantId, Guid employeeId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.TalentEmployeeMarkedHighPotential,
            $"{employeeName} was marked as high potential",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"talent-employee-marked-high-potential:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTalentReviewTemplateCreated(
        string tenantId, Guid templateId, string templateName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.TalentReviewTemplateCreated,
            $"Talent review template \"{templateName}\" was created",
            "talent-review-template", templateId, templateName, currentUser,
            correlationSuffix: $"talent-review-template-created:{templateId:N}");

    public static RecordTenantActivityRequest ForTalentReviewTemplateUpdated(
        string tenantId, Guid templateId, string templateName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.TalentReviewTemplateUpdated,
            $"Talent review template \"{templateName}\" was updated",
            "talent-review-template", templateId, templateName, currentUser,
            correlationSuffix: $"talent-review-template-updated:{templateId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTalentReviewTemplateDeleted(
        string tenantId, Guid templateId, string templateName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.TalentReviewTemplateDeleted,
            $"Talent review template \"{templateName}\" was deleted",
            "talent-review-template", templateId, templateName, currentUser,
            correlationSuffix: $"talent-review-template-deleted:{templateId:N}");

    public static RecordTenantActivityRequest ForTalentReviewTemplateActivated(
        string tenantId, Guid templateId, string templateName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.TalentReviewTemplateActivated,
            $"Talent review template \"{templateName}\" was activated",
            "talent-review-template", templateId, templateName, currentUser,
            correlationSuffix: $"talent-review-template-activated:{templateId:N}");

    public static RecordTenantActivityRequest ForTalentReviewTemplateDeactivated(
        string tenantId, Guid templateId, string templateName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.TalentReviewTemplateDeactivated,
            $"Talent review template \"{templateName}\" was deactivated",
            "talent-review-template", templateId, templateName, currentUser,
            correlationSuffix: $"talent-review-template-deactivated:{templateId:N}");

    // ── Leave extras ───────────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForTravelPermissionCreated(
        string tenantId, Guid permissionId, string summary, Guid actorUserId, string actorUserName) =>
        BuildManual(tenantId, TenantActivityModules.Hr, HrLeaveActivityTypes.TravelPermissionCreated,
            summary, "travel-permission", permissionId, summary, actorUserId, actorUserName,
            correlationSuffix: $"travel-permission-created:{permissionId:N}");

    public static RecordTenantActivityRequest ForTravelPermissionApproved(
        string tenantId, Guid permissionId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLeaveActivityTypes.TravelPermissionApproved,
            summary, "travel-permission", permissionId, summary, currentUser,
            correlationSuffix: $"travel-permission-approved:{permissionId:N}");

    public static RecordTenantActivityRequest ForTravelPermissionRejected(
        string tenantId, Guid permissionId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLeaveActivityTypes.TravelPermissionRejected,
            summary, "travel-permission", permissionId, summary, currentUser,
            correlationSuffix: $"travel-permission-rejected:{permissionId:N}");

    public static RecordTenantActivityRequest ForEmployeeLeaveConfigured(
        string tenantId, Guid employeeId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLeaveActivityTypes.EmployeeLeaveConfigured,
            $"Leave configured for {employeeName}",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"employee-leave-configured:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForEmployeeLeaveBackfillCompleted(
        string tenantId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLeaveActivityTypes.EmployeeLeaveBackfillCompleted,
            summary, "leave-backfill", Guid.Empty, summary, currentUser,
            correlationSuffix: $"employee-leave-backfill-completed:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    // ── Recruitment ────────────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForApplicationSubmitted(
        string tenantId, Guid applicationId, string applicantName, Guid actorUserId, string actorUserName) =>
        BuildManual(tenantId, TenantActivityModules.Hr, HrRecruitmentActivityTypes.ApplicationSubmitted,
            $"{applicantName} submitted an application",
            "application", applicationId, applicantName, actorUserId, actorUserName,
            correlationSuffix: $"application-submitted:{applicationId:N}");

    public static RecordTenantActivityRequest ForInternalApplicationSubmitted(
        string tenantId, Guid applicationId, string applicantName, Guid actorUserId, string actorUserName) =>
        BuildManual(tenantId, TenantActivityModules.Hr, HrRecruitmentActivityTypes.InternalApplicationSubmitted,
            $"{applicantName} submitted an internal application",
            "application", applicationId, applicantName, actorUserId, actorUserName,
            correlationSuffix: $"internal-application-submitted:{applicationId:N}");

    public static RecordTenantActivityRequest ForInterviewCompleted(
        string tenantId, Guid interviewId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrRecruitmentActivityTypes.InterviewCompleted,
            summary, "interview", interviewId, summary, currentUser,
            correlationSuffix: $"interview-completed:{interviewId:N}");

    public static RecordTenantActivityRequest ForApplicationRejectedAfterInterview(
        string tenantId, Guid applicationId, string applicantName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrRecruitmentActivityTypes.ApplicationRejectedAfterInterview,
            $"{applicantName}'s application was rejected after interview",
            "application", applicationId, applicantName, currentUser,
            correlationSuffix: $"application-rejected-after-interview:{applicationId:N}");

    public static RecordTenantActivityRequest ForOfferApprovalStepApproved(
        string tenantId, Guid offerId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrRecruitmentActivityTypes.OfferApprovalStepApproved,
            summary, "offer", offerId, summary, currentUser,
            correlationSuffix: $"offer-approval-step-approved:{offerId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForOfferFullyApproved(
        string tenantId, Guid offerId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrRecruitmentActivityTypes.OfferFullyApproved,
            summary, "offer", offerId, summary, currentUser,
            correlationSuffix: $"offer-fully-approved:{offerId:N}");

    public static RecordTenantActivityRequest ForCandidateHired(
        string tenantId, Guid hireId, string candidateName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrRecruitmentActivityTypes.CandidateHired,
            $"{candidateName} was hired",
            "hire", hireId, candidateName, currentUser,
            correlationSuffix: $"candidate-hired:{hireId:N}");

    // ── Payroll ────────────────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForPayRunCreated(
        string tenantId, Guid payRunId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayRunCreated,
            summary, "pay-run", payRunId, summary, currentUser,
            correlationSuffix: $"pay-run-created:{payRunId:N}");

    public static RecordTenantActivityRequest ForPayRunApproved(
        string tenantId, Guid payRunId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayRunApproved,
            summary, "pay-run", payRunId, summary, currentUser,
            correlationSuffix: $"pay-run-approved:{payRunId:N}");

    public static RecordTenantActivityRequest ForPayRunProcessed(
        string tenantId, Guid payRunId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayRunProcessed,
            summary, "pay-run", payRunId, summary, currentUser,
            correlationSuffix: $"pay-run-processed:{payRunId:N}");

    public static RecordTenantActivityRequest ForPaymentCreated(
        string tenantId, Guid paymentId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PaymentCreated,
            summary, "payment", paymentId, summary, currentUser,
            correlationSuffix: $"payment-created:{paymentId:N}");

    public static RecordTenantActivityRequest ForPaymentProcessed(
        string tenantId, Guid paymentId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PaymentProcessed,
            summary, "payment", paymentId, summary, currentUser,
            correlationSuffix: $"payment-processed:{paymentId:N}");

    public static RecordTenantActivityRequest ForPayrollRunPaymentsProcessed(
        string tenantId, Guid payRunId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayrollRunPaymentsProcessed,
            summary, "pay-run", payRunId, summary, currentUser,
            correlationSuffix: $"payroll-run-payments-processed:{payRunId:N}");

    public static RecordTenantActivityRequest ForClaimApproved(
        string tenantId, Guid claimId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.ClaimApproved,
            summary, "claim", claimId, summary, currentUser,
            correlationSuffix: $"claim-approved:{claimId:N}");

    public static RecordTenantActivityRequest ForClaimRejected(
        string tenantId, Guid claimId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.ClaimRejected,
            summary, "claim", claimId, summary, currentUser,
            correlationSuffix: $"claim-rejected:{claimId:N}");

    public static RecordTenantActivityRequest ForClaimPaymentProcessed(
        string tenantId, Guid claimId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.ClaimPaymentProcessed,
            summary, "claim", claimId, summary, currentUser,
            correlationSuffix: $"claim-payment-processed:{claimId:N}");

    public static RecordTenantActivityRequest ForClaimProcessed(
        string tenantId, Guid claimId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.ClaimProcessed,
            summary, "claim", claimId, summary, currentUser,
            correlationSuffix: $"claim-processed:{claimId:N}");

    public static RecordTenantActivityRequest ForAdvanceApproved(
        string tenantId, Guid advanceId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.AdvanceApproved,
            summary, "advance", advanceId, summary, currentUser,
            correlationSuffix: $"advance-approved:{advanceId:N}");

    public static RecordTenantActivityRequest ForAdvanceRejected(
        string tenantId, Guid advanceId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.AdvanceRejected,
            summary, "advance", advanceId, summary, currentUser,
            correlationSuffix: $"advance-rejected:{advanceId:N}");

    public static RecordTenantActivityRequest ForLoanApproved(
        string tenantId, Guid loanId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.LoanApproved,
            summary, "loan", loanId, summary, currentUser,
            correlationSuffix: $"loan-approved:{loanId:N}");

    public static RecordTenantActivityRequest ForAllowanceAssigned(
        string tenantId, Guid employeeId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.AllowanceAssigned,
            summary, "employee", employeeId, summary, currentUser,
            correlationSuffix: $"allowance-assigned:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForDeductionAssigned(
        string tenantId, Guid employeeId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.DeductionAssigned,
            summary, "employee", employeeId, summary, currentUser,
            correlationSuffix: $"deduction-assigned:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForPayrollAdjustmentApproved(
        string tenantId, Guid adjustmentId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayrollAdjustmentApproved,
            summary, "payroll-adjustment", adjustmentId, summary, currentUser,
            correlationSuffix: $"payroll-adjustment-approved:{adjustmentId:N}");

    public static RecordTenantActivityRequest ForPayrollAdjustmentRejected(
        string tenantId, Guid adjustmentId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayrollAdjustmentRejected,
            summary, "payroll-adjustment", adjustmentId, summary, currentUser,
            correlationSuffix: $"payroll-adjustment-rejected:{adjustmentId:N}");

    public static RecordTenantActivityRequest ForPayrollItemApproved(
        string tenantId, Guid itemId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayrollItemApproved,
            summary, "payroll-item", itemId, summary, currentUser,
            correlationSuffix: $"payroll-item-approved:{itemId:N}");

    public static RecordTenantActivityRequest ForOvertimeApproved(
        string tenantId, Guid overtimeId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.OvertimeApproved,
            summary, "overtime", overtimeId, summary, currentUser,
            correlationSuffix: $"overtime-approved:{overtimeId:N}");

    public static RecordTenantActivityRequest ForOvertimeRejected(
        string tenantId, Guid overtimeId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.OvertimeRejected,
            summary, "overtime", overtimeId, summary, currentUser,
            correlationSuffix: $"overtime-rejected:{overtimeId:N}");

    public static RecordTenantActivityRequest ForProvidentFundWithdrawalApproved(
        string tenantId, Guid withdrawalId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.ProvidentFundWithdrawalApproved,
            summary, "provident-fund-withdrawal", withdrawalId, summary, currentUser,
            correlationSuffix: $"provident-fund-withdrawal-approved:{withdrawalId:N}");

    public static RecordTenantActivityRequest ForProvidentFundWithdrawalRejected(
        string tenantId, Guid withdrawalId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.ProvidentFundWithdrawalRejected,
            summary, "provident-fund-withdrawal", withdrawalId, summary, currentUser,
            correlationSuffix: $"provident-fund-withdrawal-rejected:{withdrawalId:N}");

    public static RecordTenantActivityRequest ForDirectDepositAccountVerified(
        string tenantId, Guid accountId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.DirectDepositAccountVerified,
            summary, "direct-deposit-account", accountId, summary, currentUser,
            correlationSuffix: $"direct-deposit-account-verified:{accountId:N}");

    public static RecordTenantActivityRequest ForGradeSalaryStepsUpserted(
        string tenantId, string gradeCode, int stepCount, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.GradeSalaryStepsUpserted,
            $"Grade {gradeCode} salary steps upserted ({stepCount} step{(stepCount == 1 ? "" : "s")})",
            "grade", Guid.Empty, gradeCode, currentUser,
            correlationSuffix: $"grade-salary-steps-upserted:{gradeCode}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    // ── Platform Workflow ──────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForWorkflowConfigCreated(
        string tenantId, Guid configId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Platform, PlatformWorkflowActivityTypes.WorkflowConfigCreated,
            summary, "workflow-config", configId, summary, currentUser,
            correlationSuffix: $"workflow-config-created:{configId:N}");

    public static RecordTenantActivityRequest ForWorkflowConfigUpdated(
        string tenantId, Guid configId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Platform, PlatformWorkflowActivityTypes.WorkflowConfigUpdated,
            summary, "workflow-config", configId, summary, currentUser,
            correlationSuffix: $"workflow-config-updated:{configId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForWorkflowConfigDeleted(
        string tenantId, Guid configId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Platform, PlatformWorkflowActivityTypes.WorkflowConfigDeleted,
            summary, "workflow-config", configId, summary, currentUser,
            correlationSuffix: $"workflow-config-deleted:{configId:N}");

    public static RecordTenantActivityRequest ForWorkflowTemplateCreated(
        string tenantId, Guid templateId, string templateName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Platform, PlatformWorkflowActivityTypes.WorkflowTemplateCreated,
            $"Workflow template \"{templateName}\" was created",
            "workflow-template", templateId, templateName, currentUser,
            correlationSuffix: $"workflow-template-created:{templateId:N}");

    public static RecordTenantActivityRequest ForWorkflowTemplateUpdated(
        string tenantId, Guid templateId, string templateName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Platform, PlatformWorkflowActivityTypes.WorkflowTemplateUpdated,
            $"Workflow template \"{templateName}\" was updated",
            "workflow-template", templateId, templateName, currentUser,
            correlationSuffix: $"workflow-template-updated:{templateId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForWorkflowTemplateDeleted(
        string tenantId, Guid templateId, string templateName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Platform, PlatformWorkflowActivityTypes.WorkflowTemplateDeleted,
            $"Workflow template \"{templateName}\" was deleted",
            "workflow-template", templateId, templateName, currentUser,
            correlationSuffix: $"workflow-template-deleted:{templateId:N}");

    public static RecordTenantActivityRequest ForWorkflowTemplateDuplicated(
        string tenantId, Guid templateId, string templateName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Platform, PlatformWorkflowActivityTypes.WorkflowTemplateDuplicated,
            $"Workflow template \"{templateName}\" was duplicated",
            "workflow-template", templateId, templateName, currentUser,
            correlationSuffix: $"workflow-template-duplicated:{templateId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForWorkflowTemplateImported(
        string tenantId, Guid templateId, string templateName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Platform, PlatformWorkflowActivityTypes.WorkflowTemplateImported,
            $"Workflow template \"{templateName}\" was imported",
            "workflow-template", templateId, templateName, currentUser,
            correlationSuffix: $"workflow-template-imported:{templateId:N}");

    // ── IAM ────────────────────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForUserCreated(
        string tenantId, Guid userId, string userName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.UserCreated,
            $"User {userName} was created",
            "user", userId, userName, currentUser,
            correlationSuffix: $"user-created:{userId:N}");

    public static RecordTenantActivityRequest ForUserActivated(
        string tenantId, Guid userId, string userName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.UserActivated,
            $"User {userName} was activated",
            "user", userId, userName, currentUser,
            correlationSuffix: $"user-activated:{userId:N}");

    public static RecordTenantActivityRequest ForUserDeactivated(
        string tenantId, Guid userId, string userName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.UserDeactivated,
            $"User {userName} was deactivated",
            "user", userId, userName, currentUser,
            correlationSuffix: $"user-deactivated:{userId:N}");

    public static RecordTenantActivityRequest ForUserPasswordReset(
        string tenantId, Guid userId, string userName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.UserPasswordReset,
            $"Password reset for user {userName}",
            "user", userId, userName, currentUser,
            correlationSuffix: $"user-password-reset:{userId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForUserProfileUpdated(
        string tenantId, Guid userId, string userName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.UserProfileUpdated,
            $"Profile updated for user {userName}",
            "user", userId, userName, currentUser,
            correlationSuffix: $"user-profile-updated:{userId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForUserRoleAssigned(
        string tenantId, Guid userId, string userName, string roleName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.UserRoleAssigned,
            $"Role \"{roleName}\" assigned to user {userName}",
            "user", userId, userName, currentUser,
            correlationSuffix: $"user-role-assigned:{userId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForUserRoleRemoved(
        string tenantId, Guid userId, string userName, string roleName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.UserRoleRemoved,
            $"Role \"{roleName}\" removed from user {userName}",
            "user", userId, userName, currentUser,
            correlationSuffix: $"user-role-removed:{userId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForRoleCreated(
        string tenantId, Guid roleId, string roleName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.RoleCreated,
            $"Role \"{roleName}\" was created",
            "role", roleId, roleName, currentUser,
            correlationSuffix: $"role-created:{roleId:N}");

    public static RecordTenantActivityRequest ForRoleDeleted(
        string tenantId, Guid roleId, string roleName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.RoleDeleted,
            $"Role \"{roleName}\" was deleted",
            "role", roleId, roleName, currentUser,
            correlationSuffix: $"role-deleted:{roleId:N}");

    public static RecordTenantActivityRequest ForRolePermissionsAssigned(
        string tenantId, Guid roleId, string roleName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.RolePermissionsAssigned,
            $"Permissions assigned to role \"{roleName}\"",
            "role", roleId, roleName, currentUser,
            correlationSuffix: $"role-permissions-assigned:{roleId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForRolePermissionRemoved(
        string tenantId, Guid roleId, string roleName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.RolePermissionRemoved,
            $"Permission removed from role \"{roleName}\"",
            "role", roleId, roleName, currentUser,
            correlationSuffix: $"role-permission-removed:{roleId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTenantRegistered(
        string tenantId, string tenantName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.TenantRegistered,
            $"Tenant \"{tenantName}\" was registered",
            "tenant", Guid.Empty, tenantName, currentUser,
            correlationSuffix: $"tenant-registered:{tenantId}");

    public static RecordTenantActivityRequest ForTenantLogoUpdated(
        string tenantId, string tenantName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.TenantLogoUpdated,
            $"Logo updated for tenant \"{tenantName}\"",
            "tenant", Guid.Empty, tenantName, currentUser,
            correlationSuffix: $"tenant-logo-updated:{tenantId}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTenantThemeUpdated(
        string tenantId, string tenantName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.TenantThemeUpdated,
            $"Theme updated for tenant \"{tenantName}\"",
            "tenant", Guid.Empty, tenantName, currentUser,
            correlationSuffix: $"tenant-theme-updated:{tenantId}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTenantSubscriptionUpdated(
        string tenantId, string tenantName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.TenantSubscriptionUpdated,
            $"Subscription updated for tenant \"{tenantName}\"",
            "tenant", Guid.Empty, tenantName, currentUser,
            correlationSuffix: $"tenant-subscription-updated:{tenantId}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    private static RecordTenantActivityRequest BuildManual(
        string tenantId, string module, string activityType, string summary,
        string subjectType, Guid subjectId, string subjectLabel,
        Guid actorUserId, string actorUserName, string correlationSuffix) =>
        new()
        {
            TenantId = tenantId,
            Module = module,
            ActivityType = activityType,
            Summary = summary,
            ActorUserId = actorUserId,
            ActorUserName = actorUserName,
            SubjectType = subjectType,
            SubjectId = subjectId,
            SubjectLabel = subjectLabel,
            OccurredAt = DateTime.UtcNow,
            CorrelationId = $"{module}:{correlationSuffix}"
        };

    private static RecordTenantActivityRequest Build(
        string tenantId,
        string module,
        string activityType,
        string summary,
        string subjectType,
        Guid subjectId,
        string subjectLabel,
        ICurrentUserService currentUser,
        string correlationSuffix)
    {
        var actorUserId = Guid.TryParse(currentUser.GetUserId(), out var parsed)
            ? parsed
            : Guid.Empty;

        return new RecordTenantActivityRequest
        {
            TenantId = tenantId,
            Module = module,
            ActivityType = activityType,
            Summary = summary,
            ActorUserId = actorUserId,
            ActorUserName = string.IsNullOrWhiteSpace(currentUser.GetUserName())
                ? currentUser.GetUserEmail()
                : currentUser.GetUserName(),
            SubjectType = subjectType,
            SubjectId = subjectId,
            SubjectLabel = subjectLabel,
            OccurredAt = DateTime.UtcNow,
            CorrelationId = $"{module}:{correlationSuffix}"
        };
    }

    private static string BuildEmployeeJoinedSummary(string employeeName, string? organizationalUnitName)
    {
        var destination = string.IsNullOrWhiteSpace(organizationalUnitName)
            ? "the organization"
            : organizationalUnitName.Trim();
        return $"New employee {employeeName} joined {destination}";
    }
}
