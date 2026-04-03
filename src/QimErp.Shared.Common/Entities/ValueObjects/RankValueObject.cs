namespace QimErp.Shared.Common.Entities.ValueObjects;

/// <summary>
/// Value object representing a Rank for use across modules.
/// Contains the essential rank information needed for employee rank references.
/// </summary>
public class RankValueObject
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string Name { get; set; } = string.Empty;

    public RankValueObject() { }

    public RankValueObject(Guid id, string name, string? code = null)
    {
        Id   = id;
        Name = name;
        Code = code;
    }

    public static RankValueObject Create(Guid id, string name, string? code = null)
        => new(id, name, code);
}
