namespace QimErp.Shared.Common.Activities;

/// <summary>
/// Payload for the EmployeeAssignmentChangedWorkflow.
/// Carries either a job-title or org-unit change — discriminated by EventType.
/// </summary>
public class EmployeeAssignmentChangedRequest
{
    /// <summary>"JobTitleChanged" or "OrganizationalUnitChanged"</summary>
    public string EventType { get; set; } = string.Empty;

    public Guid EmployeeId { get; set; }
    public string TenantId { get; set; } = string.Empty;

    // Job-title fields (populated when EventType == "JobTitleChanged")
    public Guid? NewJobTitleId   { get; set; }
    public string? NewJobTitleName { get; set; }
    public string? NewJobTitleCode { get; set; }
    public Guid? OldJobTitleId   { get; set; }

    // Org-unit fields (populated when EventType == "OrganizationalUnitChanged")
    public Guid? NewOrgUnitId   { get; set; }
    public string? NewOrgUnitName { get; set; }
    public string? NewOrgUnitCode { get; set; }
    public Guid? OldOrgUnitId   { get; set; }

    // Station fields (populated when EventType == "StationChanged")
    public Guid? NewStationId   { get; set; }
    public string? NewStationName { get; set; }
    public string? NewStationCode { get; set; }
    public Guid? OldStationId   { get; set; }

    // Job-status fields (populated when EventType == "JobStatusChanged")
    public Guid? NewJobStatusId   { get; set; }
    public string? NewJobStatusName { get; set; }
    public string? NewJobStatusCode { get; set; }
    public Guid? OldJobStatusId   { get; set; }
}
