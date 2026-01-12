namespace QimErp.Shared.Common.Entities;

/// <summary>
/// Base class for CostCenter entities across all modules.
/// Contains common properties and methods shared by module-specific CostCenter entities.
/// </summary>
public abstract class CostCenterBase : GuidAuditableEntity
{
    public string Code { get; protected set; } = string.Empty;
    public string Name { get; protected set; } = string.Empty;
    public string? Description { get; protected set; }

    // Computed Properties
    public bool IsActive => DataStatus == DataState.Active;

    protected CostCenterBase() { }

    protected CostCenterBase(
        Guid id,
        string code,
        string name,
        string? description = null)
    {
        Id = id;
        Code = code;
        Name = name;
        Description = description;
        AsActive();
    }

    /// <summary>
    /// Updates cost center information
    /// </summary>
    public CostCenterBase UpdateInfo(string code, string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Cost center code is required", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Cost center name is required", nameof(name));

        Code = code;
        Name = name;
        Description = description;
        return this;
    }

    /// <summary>
    /// Activates the cost center
    /// </summary>
    public CostCenterBase Activate()
    {
        AsActive();
        return this;
    }

    /// <summary>
    /// Deactivates the cost center
    /// </summary>
    public new CostCenterBase Deactivate()
    {
        base.Deactivate();
        return this;
    }
}
