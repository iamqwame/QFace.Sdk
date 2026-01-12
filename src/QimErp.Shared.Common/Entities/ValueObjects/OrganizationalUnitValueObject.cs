namespace QimErp.Shared.Common.Entities.ValueObjects;

/// <summary>
/// Value object representing an Organizational Unit for use in other modules.
/// Contains essential organizational unit information needed for organizational structure references.
/// </summary>
public class OrganizationalUnitValueObject
{
    // Required for organizational unit references
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string Name { get; set; } = string.Empty;
    
    // Optional - useful for display/description
    public string? Description { get; set; }
    
    // Constructors
    public OrganizationalUnitValueObject() { }
    
    public OrganizationalUnitValueObject(
        Guid id,
        string name,
        string? code = null,
        string? description = null)
    {
        Id = id;
        Name = name;
        Code = code;
        Description = description;
    }

    public static OrganizationalUnitValueObject Create(
        Guid id,
        string name,
        string? code = null,
        string? description = null)
    {
        return new OrganizationalUnitValueObject(id, name, code, description);
    }
}
