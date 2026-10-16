#pragma warning disable CS0618 // Dual-publish bridge still references EmployeeChangedEvent
namespace QimErp.Shared.Common.Activities;

/// <summary>
/// Request payload for the EmployeeBulkSyncWorkflow.
/// Used during bulk imports to sync a batch of employees to all modules in a single
/// workflow execution instead of firing one EmployeeSyncWorkflow per employee.
/// </summary>
public class EmployeeBulkSyncRequest
{
    /// <summary>Legacy fat batch (dual-publish). Prefer <see cref="EmployeesV2"/>.</summary>
    public List<EmployeeChangedEvent> Employees { get; set; } = [];

    /// <summary>Ids-only batch (DATA-11). Keep under 200 entries to stay within Temporal payload limits.</summary>
    public List<EmployeeChangedEventV2> EmployeesV2 { get; set; } = [];

    /// <summary>Tenant scope — required for all module inserts.</summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>Import job that triggered this bulk sync — used for traceability in workflow IDs.</summary>
    public string ImportJobId { get; set; } = string.Empty;
}
