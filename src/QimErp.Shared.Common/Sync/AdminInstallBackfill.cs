namespace QimErp.Shared.Common.Sync;

/// <summary>Temporal activity names and queue for CoreHR admin reference install backfill.</summary>
public static class AdminInstallBackfill
{
    public const string TaskQueue = EmployeeInstallBackfill.TaskQueue;
    public const string CountsActivity = "GetAdminBackfillCounts";
    public const string LoadBatchActivity = "LoadAdminBackfillBatch";
}

/// <summary>Active admin entity counts in CoreHR for one tenant.</summary>
public sealed class AdminBackfillCounts
{
    public int JobTitles { get; set; }
    public int JobStatuses { get; set; }
    public int OrganizationalUnits { get; set; }
    public int Stations { get; set; }
    public int Ranks { get; set; }

    public int Total => JobTitles + JobStatuses + OrganizationalUnits + Stations + Ranks;
}
