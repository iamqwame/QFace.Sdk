namespace QimErp.Shared.Common.Entities.ValueObjects;

public class ProductValueObject
{
    public ProductValueObject(string id, string code, string name,
        string? sku = null, string? imageUrl = null)
    {
        Id = id;
        Code = code;
        Name = name;
        Sku = sku;
        ImageUrl = imageUrl;
    }
    public ProductValueObject()
    {

    }

    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public string? ImageUrl { get; set; }

    public string? Category { get; set; }
    public string? CategoryId { get; set; }
    public string? Brand { get; set; }
    public string? Measurement { get; set; }
    public string? BarcodeGtin { get; set; }

    public static ProductValueObject Create(string id, string code, string name, string? sku = null, string? imageUrl = null)
    {
        return new ProductValueObject(id, code, name, sku, imageUrl);
    }
}
