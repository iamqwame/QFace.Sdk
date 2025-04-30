using MongoDB.Bson.Serialization.Attributes;

namespace Demo.MongoDb.MultiTenant.Api;

/// <summary>
/// Product document (tenant-specific data)
/// </summary>
[BsonDiscriminator("products")] 
public class Product : TenantBaseDocument
{
    /// <summary>
    /// Product name
    /// </summary>
    public string Name { get; set; } = string.Empty;
        
    /// <summary>
    /// Product description
    /// </summary>
    public string Description { get; set; } = string.Empty;
        
    /// <summary>
    /// Product price
    /// </summary>
    public decimal Price { get; set; }
        
    /// <summary>
    /// Available quantity
    /// </summary>
    public int StockQuantity { get; set; }
        
    /// <summary>
    /// SKU (Stock Keeping Unit)
    /// </summary>
    public string SKU { get; set; } = string.Empty;
        
    /// <summary>
    /// Categories this product belongs to
    /// </summary>
    public List<string> Categories { get; set; } = new();
}