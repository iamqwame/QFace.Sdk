namespace QimErp.Shared.Common.Entities;

/// <summary>
/// Base class for all lookup/value reference tables.
/// Provides common properties for configurable enum replacements.
/// </summary>
public abstract class LookupBase : GuidAuditableEntity
{
    /// <summary>
    /// Unique code for the lookup value (e.g., "HOUSING", "TECHNICAL")
    /// Must be unique within the lookup type and tenant.
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// Display name of the lookup value (e.g., "Housing Allowance", "Technical")
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Optional description of the lookup value
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Display order for sorting lookups in UI
    /// </summary>
    public int DisplayOrder { get; private set; }

    /// <summary>
    /// Indicates if this is a system default value that cannot be deleted
    /// </summary>
    public bool IsSystemDefault { get; private set; }

    /// <summary>
    /// Type identifier for the lookup (e.g., "AllowanceType", "CourseCategory")
    /// Used to differentiate lookup types when using a shared table approach
    /// </summary>
    public string LookupType { get; private set; } = string.Empty;

    protected LookupBase() { }

    protected LookupBase(string code, string name, string lookupType)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be null or empty", nameof(code));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));
        
        if (string.IsNullOrWhiteSpace(lookupType))
            throw new ArgumentException("LookupType cannot be null or empty", nameof(lookupType));

        Id = CreateId();
        Code = code.ToUpperInvariant(); // Store codes in uppercase for consistency
        Name = name;
        LookupType = lookupType;
        DisplayOrder = 0;
        IsSystemDefault = false;
        AsActive();
    }

    /// <summary>
    /// Creates a system default lookup value
    /// </summary>
    protected static T CreateSystemDefault<T>(string code, string name, string lookupType, int displayOrder = 0)
        where T : LookupBase
    {
        // Use reflection to create instance via protected parameterless constructor
        var lookup = (T)Activator.CreateInstance(typeof(T), BindingFlags.NonPublic | BindingFlags.Instance, null, null, null)!;
        lookup.Id = CreateId();
        lookup.Code = code.ToUpperInvariant();
        lookup.Name = name;
        lookup.LookupType = lookupType;
        lookup.DisplayOrder = displayOrder;
        lookup.IsSystemDefault = true;
        lookup.AsActive();
        return lookup;
    }

    public LookupBase UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));

        if (IsSystemDefault)
            throw new InvalidOperationException("Cannot update name of system default lookup");

        Name = name;
        return this;
    }

    public LookupBase UpdateDescription(string? description)
    {
        Description = description;
        return this;
    }

    public LookupBase UpdateDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
        return this;
    }

    public LookupBase MarkAsSystemDefault()
    {
        IsSystemDefault = true;
        return this;
    }
}

