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
