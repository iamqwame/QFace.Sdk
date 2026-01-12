namespace QimErp.Shared.Common.Entities;

/// <summary>
/// Base class for OrganizationalUnit entities across all modules.
/// Contains common properties and methods shared by module-specific OrganizationalUnit entities.
/// </summary>
public abstract class OrganizationalUnitBase : GuidAuditableEntity
{
    public string Name { get; protected set; } = string.Empty;
    public string? Code { get; protected set; }
    public string? Description { get; protected set; }

    // Computed Properties
    public bool IsActive => DataStatus == DataState.Active;

    protected OrganizationalUnitBase() { }

    protected OrganizationalUnitBase(
        Guid id,
        string name,
        string? code = null,
        string? description = null)
    {
        Id = id;
        Name = name;
        Code = code;
        Description = description;
        AsActive();
    }

    /// <summary>
    /// Updates organizational unit information
    /// </summary>
    public OrganizationalUnitBase UpdateInfo(string name, string? code = null, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Organizational unit name is required", nameof(name));

        Name = name;
        Code = code;
        Description = description;
        return this;
    }

    /// <summary>
    /// Activates the organizational unit
    /// </summary>
    public OrganizationalUnitBase Activate()
    {
        AsActive();
        return this;
    }

    /// <summary>
    /// Deactivates the organizational unit
    /// </summary>
    public new OrganizationalUnitBase Deactivate()
    {
        base.Deactivate();
        return this;
    }
}
