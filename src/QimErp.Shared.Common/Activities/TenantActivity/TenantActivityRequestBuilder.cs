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

    // ── Payroll ────────────────────────────────────────────────────────────
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
