namespace QimErp.Shared.Common.Sync;

/// <summary>
/// Modules that have migrated to <see cref="Events.EmployeeChangedEventV2"/> activity names
/// (<c>SyncEmployeeCreatedV2:{Suffix}</c>, etc.). Unlisted modules still receive V1 dual-publish.
/// </summary>
public static class EmployeeSyncV2Modules
{
    private static readonly HashSet<string> Suffixes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Learning",
        "Performance",
        "Talent",
        "WorkforcePlanning",
        "Payroll",
        "TenantBilling",
        "Leave",
        "Benefit",
        "Surveys"
    };

    public static bool UsesV2(string activitySuffix) =>
        !string.IsNullOrWhiteSpace(activitySuffix) && Suffixes.Contains(activitySuffix);
}
