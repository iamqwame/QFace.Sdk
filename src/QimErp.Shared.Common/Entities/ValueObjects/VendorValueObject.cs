namespace QimErp.Shared.Common.Entities.ValueObjects;

public class VendorValueObject
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public VendorValueObject()
    {
    }

    public VendorValueObject(string id, string name, string? code = null)
    {
        Id = id;
        Name = name;
        Code = code ?? string.Empty;
    }

    public static VendorValueObject Create(string id, string name, string? code = null) =>
        new(id, name, code);
}
