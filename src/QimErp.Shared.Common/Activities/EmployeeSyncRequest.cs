#pragma warning disable CS0618 // Dual-publish bridge still references EmployeeChangedEvent
namespace QimErp.Shared.Common.Activities;

public enum EmployeeSyncOperation { Created, Updated, Deleted, Terminated }

public class EmployeeSyncRequest
{
    public EmployeeSyncOperation Operation { get; set; }

    /// <summary>
    /// Legacy fat payload (DATA-11 dual-publish). Prefer <see cref="EmployeeV2"/>.
    /// Unmigrated consumers (e.g. IAM) still receive this until they move to V2 + owner fetch.
    /// </summary>
    public EmployeeChangedEvent? Employee { get; set; }

    /// <summary>Ids-only employee change (DATA-11). Populated for Created/Updated/Terminated.</summary>
    public EmployeeChangedEventV2? EmployeeV2 { get; set; }

    /// <summary>Always populated — used as the primary key for the sync.</summary>
    public Guid EmployeeId { get; set; }

    /// <summary>Tenant scope — required for all operations.</summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Employee email — legacy Deleted path for IAM lookup. Prefer owner fetch when migrating off V1.
    /// </summary>
    public string EmployeeEmail { get; set; } = string.Empty;

    /// <summary>Tenant installed modules — loaded by orchestrator when not supplied by caller.</summary>
    public List<string>? SelectedModules { get; set; }
}
