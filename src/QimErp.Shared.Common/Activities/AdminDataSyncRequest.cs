namespace QimErp.Shared.Common.Activities;

public enum AdminSyncOperation { CreatedOrUpdated, Deleted }

public enum AdminEntityType { JobTitle, JobStatus, OrganizationalUnit, Station }

/// <summary>
/// Payload for AdminDataSyncWorkflow fan-out activities.
/// Carries the minimum information needed by each module to upsert or deactivate
/// its local copy of a CoreHR admin reference entity (JobTitle, JobStatus, OrgUnit, Station).
/// </summary>
public class AdminDataSyncRequest
{
    public AdminEntityType EntityType { get; set; }
    public AdminSyncOperation Operation { get; set; }
    public Guid EntityId { get; set; }
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Used by OrgUnit sync to check the organizational unit type.
    /// Department = 1 (triggers additional JobTitle OU reference updates).
    /// </summary>
    public int? OrgUnitType { get; set; }
}
