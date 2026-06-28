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
    public const string BulkOperation = "bulk-operation";
}
