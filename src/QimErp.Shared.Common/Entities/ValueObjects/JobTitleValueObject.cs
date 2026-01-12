namespace QimErp.Shared.Common.Entities.ValueObjects;

/// <summary>
/// Value object representing a Job Title for use in other modules.
/// Contains essential job title information needed for job title references.
/// </summary>
public class JobTitleValueObject
{
    // Required for job title references
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string Name { get; set; } = string.Empty;
    
    // Optional - useful for display/description
    public string? Description { get; set; }
    
    // Constructors
    public JobTitleValueObject() { }
    
    public JobTitleValueObject(
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

    public static JobTitleValueObject Create(
        Guid id,
        string name,
        string? code = null,
        string? description = null)
    {
        return new JobTitleValueObject(id, name, code, description);
    }
}
