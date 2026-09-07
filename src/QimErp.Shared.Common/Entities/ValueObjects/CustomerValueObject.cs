namespace QimErp.Shared.Common.Entities.ValueObjects;

public class CustomerValueObject
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public CustomerValueObject()
    {
    }

    public CustomerValueObject(string id, string name, string? code = null)
    {
        Id = id;
        Name = name;
        Code = code ?? string.Empty;
    }

    public static CustomerValueObject Create(string id, string name, string? code = null) =>
        new(id, name, code);
}
