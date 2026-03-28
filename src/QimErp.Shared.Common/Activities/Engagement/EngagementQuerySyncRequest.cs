namespace QimErp.Shared.Common.Activities.Engagement;

public class EngagementQuerySyncRequest
{
    public Guid EntityId       { get; set; }
    public string EntityCode   { get; set; } = "";
    public string EntityTitle  { get; set; } = "";
    public string? Description { get; set; }
    public Guid EmployeeId     { get; set; }
    /// <summary>Extra context appended to the EmployeeQuery description (e.g. "Level: High, Score: 75").</summary>
    public string? ExtraInfo   { get; set; }
    public string TenantId     { get; set; } = "";
    public string TriggeredBy  { get; set; } = "";
    public string? UserName    { get; set; }
}
