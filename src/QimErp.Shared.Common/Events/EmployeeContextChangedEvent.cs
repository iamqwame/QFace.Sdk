namespace QimErp.Shared.Common.Events;

public class EmployeeContextChangedEvent : DomainEvent
{
    public string UserEmail { get; set; } = string.Empty;
    public string? EmployeeId { get; set; }
    public string? RankId { get; set; }
    public string? RankName { get; set; }
    public string? OrganizationalUnitId { get; set; }
    public string? OrganizationalUnitName { get; set; }
    public List<string> RoleIds { get; set; } = [];

    public EmployeeContextChangedEvent()
    {
    }

    public EmployeeContextChangedEvent(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        UserEmail = userEmail;
    }

    public static EmployeeContextChangedEvent Create(
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new EmployeeContextChangedEvent(tenantId, userEmail, triggeredBy, userName);
    }

    public EmployeeContextChangedEvent WithEmployeeId(string? id)
    {
        EmployeeId = id;
        return this;
    }

    public EmployeeContextChangedEvent WithRank(string? id, string? name)
    {
        RankId = id;
        RankName = name;
        return this;
    }

    public EmployeeContextChangedEvent WithOrganizationalUnit(string? id, string? name)
    {
        OrganizationalUnitId = id;
        OrganizationalUnitName = name;
        return this;
    }

    public EmployeeContextChangedEvent WithRoleIds(List<string> roleIds)
    {
        RoleIds = roleIds;
        return this;
    }
}
