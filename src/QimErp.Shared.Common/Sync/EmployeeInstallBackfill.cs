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
