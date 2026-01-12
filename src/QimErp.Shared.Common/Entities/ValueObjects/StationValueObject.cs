namespace QimErp.Shared.Common.Entities.ValueObjects;

/// <summary>
/// Value object representing a Station (Location) for use in other modules.
/// Contains essential station information needed for location references.
/// </summary>
public class StationValueObject
{
    // Required for station references
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string Name { get; set; } = string.Empty;
    
    // Constructors
    public StationValueObject() { }
    
    public StationValueObject(
        Guid id,
        string name,
        string? code = null)
    {
        Id = id;
        Name = name;
        Code = code;
    }

    public static StationValueObject Create(
        Guid id,
        string name,
        string? code = null)
    {
        return new StationValueObject(id, name, code);
    }
}
