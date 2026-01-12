namespace QimErp.Shared.Common.Entities.ValueObjects;

/// <summary>
/// Value object representing a Job Status for use in other modules.
/// Contains essential job status information needed for job status references.
/// </summary>
public class JobStatusValueObject
{
    // Required for job status references
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string Name { get; set; } = string.Empty;
    
    // Optional - useful for display/description
    public string? Description { get; set; }
    
    // Constructors
    public JobStatusValueObject() { }
    
    public JobStatusValueObject(
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

    public static JobStatusValueObject Create(
        Guid id,
        string name,
        string? code = null,
        string? description = null)
    {
        return new JobStatusValueObject(id, name, code, description);
    }
}
