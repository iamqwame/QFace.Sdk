namespace QimErp.Shared.Common.Activities.TenantActivity;

public static class TenantActivityModules
{
    public const string Hr = "hr";
    public const string Accounting = "accounting";
    public const string Iam = "iam";
}

public static class HrActivityTypes
{
    public const string EmployeeCreated = "employee-created";
    public const string EmployeeUpdated = "employee-updated";
    public const string EmployeeDeactivated = "employee-deactivated";
    public const string DepartmentCreated = "department-created";
    public const string DepartmentUpdated = "department-updated";
    public const string DepartmentDeleted = "department-deleted";
    public const string JobTitleCreated = "job-title-created";
    public const string JobTitleUpdated = "job-title-updated";
    public const string JobTitleDeleted = "job-title-deleted";
    public const string LeaveRequestSubmitted = "leave-request-submitted";
    public const string LeaveRequestApproved = "leave-request-approved";
    public const string LeaveRequestRejected = "leave-request-rejected";
    public const string LeaveRequestCancelled = "leave-request-cancelled";
    public const string OffboardingInitiated = "offboarding-initiated";
    public const string OffboardingApproved = "offboarding-approved";
    public const string OffboardingCancelled = "offboarding-cancelled";
    public const string OffboardingCompleted = "offboarding-completed";
    public const string EmployeeJobTitleChanged = "employee-job-title-changed";
    public const string EmployeeOrgUnitChanged = "employee-org-unit-changed";
    public const string BulkOperation = "bulk-operation";
}

public static class PayrollActivityTypes
{
    public const string GradeBulkAssigned = "grade-bulk-assigned";
    public const string PayRunCompleted = "pay-run-completed";
}
