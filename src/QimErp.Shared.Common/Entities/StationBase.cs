namespace QimErp.Shared.Common.Entities;

/// <summary>
/// Base class for Station entities across all modules.
/// Contains common properties and methods shared by module-specific Station entities.
/// </summary>
public abstract class StationBase : GuidAuditableEntity
{
    public string Name { get; protected set; } = string.Empty;
    public string? Code { get; protected set; }
    public Guid? OrganizationalUnitId { get; protected set; }

    // Computed Properties
    public bool IsActive => DataStatus == DataState.Active;

    protected StationBase() { }

    protected StationBase(
        Guid id,
        string name,
        string? code = null)
    {
        Id = id;
        Name = name;
        Code = code;
        AsActive();
    }

    /// <summary>
    /// Updates station information
    /// </summary>
    public StationBase UpdateInfo(string name, string? code = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Station name is required", nameof(name));

        Name = name;
        Code = code;
        return this;
    }

    /// <summary>
    /// Sets the organizational unit reference
    /// </summary>
    public StationBase WithOrganizationalUnit(Guid? organizationalUnitId)
    {
        OrganizationalUnitId = organizationalUnitId;
        return this;
    }

    /// <summary>
    /// Activates the station
    /// </summary>
    public StationBase Activate()
    {
        AsActive();
        return this;
    }

    /// <summary>
    /// Deactivates the station
    /// </summary>
    public new StationBase Deactivate()
    {
        base.Deactivate();
        return this;
    }
}
