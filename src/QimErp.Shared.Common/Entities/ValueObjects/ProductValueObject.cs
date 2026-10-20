namespace QimErp.Shared.Common.Entities.ValueObjects;

public class ProductValueObject
{
    public ProductValueObject(string id, string code, string name,
        string? sku = null, string? imageUrl = null, Guid? imageFileId = null)
    {
        Id = id;
        Code = code;
        Name = name;
        Sku = sku;
        if (imageFileId.HasValue)
            WithImageFileId(imageFileId);
        else
            WithImageUrl(imageUrl);
    }

    public ProductValueObject()
    {
    }

    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public string? ImageUrl { get; set; }
    public Guid? ImageFileId { get; set; }

    public string? Category { get; set; }
    public string? CategoryId { get; set; }
    public string? Brand { get; set; }
    public string? Measurement { get; set; }
    public string? BarcodeGtin { get; set; }

    /// <summary>DATA-18: persist FileId; clears legacy ImageUrl. Production writes must use this.</summary>
    public ProductValueObject WithImageFileId(Guid? imageFileId)
    {
        ImageFileId = imageFileId;
        ImageUrl = null;
        return this;
    }

    /// <summary>
    /// Legacy URL writer. Null/whitespace clears both ImageUrl and ImageFileId.
    /// Non-empty URL is a no-op (do not persist URL or synthesize FileId).
    /// </summary>
    public ProductValueObject WithImageUrl(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            ImageUrl = null;
            ImageFileId = null;
            return this;
        }

        return this;
    }

    public static ProductValueObject Create(
        string id,
        string code,
        string name,
        string? sku = null,
        string? imageUrl = null,
        Guid? imageFileId = null)
    {
        return new ProductValueObject(id, code, name, sku, imageUrl, imageFileId);
    }
}
