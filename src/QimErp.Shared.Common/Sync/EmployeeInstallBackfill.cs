namespace QimErp.Shared.Common.Sync;

/// <summary>
/// Temporal activity names and queue for CoreHR employee install backfill.
/// </summary>
public static class EmployeeInstallBackfill
{
    public const string TaskQueue = "qimerp-corehr-employee-tenant-setup";
    public const string LoadBatchActivity = "LoadEmployeeBackfillBatch";
    public const string CountActivity = "CountEmployeesForBackfill";
}

/// <summary>A bare string arg has no TenantId property for TenantContextActivityInterceptor to find — wrap it.</summary>
public sealed class EmployeeBackfillCountRequest
{
    public required string TenantId { get; set; }
}

/// <summary>A bare string arg has no TenantId property for TenantContextActivityInterceptor to find — wrap it.</summary>
public sealed class EmployeeBackfillBatchRequest
{
    public required string TenantId { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<string>? SelectedModules { get; set; }
}
