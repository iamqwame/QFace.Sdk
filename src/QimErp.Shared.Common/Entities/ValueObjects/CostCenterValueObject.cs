namespace QimErp.Shared.Common.Entities.ValueObjects;

/// <summary>
/// Value object representing a Cost Center for use in other modules.
/// Contains essential cost center information needed for cost allocation and tracking.
/// </summary>
public class CostCenterValueObject
{
    // Required for cost center references
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    
    // Optional - useful for display/description
    public string? Description { get; set; }
    
    // Constructors
    public CostCenterValueObject() { }
    
    public CostCenterValueObject(
        Guid id,
        string code,
        string name,
        string? description = null)
    {
        Id = id;
        Code = code;
        Name = name;
        Description = description;
    }
}
