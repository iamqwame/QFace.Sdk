namespace QimErp.Shared.Common.Activities;

public enum EmployeeSyncOperation { Created, Updated, Deleted }

public class EmployeeSyncRequest
{
    public EmployeeSyncOperation Operation { get; set; }

    /// <summary>Populated for Created and Updated operations.</summary>
    public EmployeeChangedEvent? Employee { get; set; }

    /// <summary>Always populated — used as the primary key for the sync.</summary>
    public Guid EmployeeId { get; set; }

    /// <summary>Tenant scope — required for all operations.</summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>Employee email — required for Deleted so IAM can look up and deactivate the user account.</summary>
    public string EmployeeEmail { get; set; } = string.Empty;
}
