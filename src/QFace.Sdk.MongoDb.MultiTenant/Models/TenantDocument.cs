namespace QFace.Sdk.MongoDb.MultiTenant.Models;

/// <summary>
/// Tenant document for storing tenant configuration
/// </summary>
public class TenantDocument : TenantBaseDocument
{
    /// <summary>
    /// Unique code/slug for the tenant (used in URLs)
    /// </summary>
    public string Code { get; set; } = string.Empty;
        
    /// <summary>
    /// Display name of the tenant
    /// </summary>
    public string Name { get; set; } = string.Empty;
        
    /// <summary>
    /// Optional description
    /// </summary>
    public string Description { get; set; } = string.Empty;
        
    /// <summary>
    /// Whether the tenant is provisioned and ready to use
    /// </summary>
    public bool IsProvisioned { get; set; }
        
    /// <summary>
    /// When the tenant was provisioned
    /// </summary>
    public DateTime? ProvisionedDate { get; set; }
        
    /// <summary>
    /// Type of tenant (shared, dedicated, etc.)
    /// </summary>
    public TenantType TenantType { get; set; } = TenantType.Dedicated;
        
    /// <summary>
    /// Connection string for tenant database (if different from default)
    /// </summary>
    public string? ConnectionString { get; set; }
        
    /// <summary>
    /// Database name for tenant
    /// </summary>
    public string? DatabaseName { get; set; }
        
    /// <summary>
    /// Tenant subscription information
    /// </summary>
    public SubscriptionInfo Subscription { get; set; } = new SubscriptionInfo();
        
    /// <summary>
    /// Contact information
    /// </summary>
    public ContactInfo Contact { get; set; } = new ContactInfo();
        
    /// <summary>
    /// Tenant configuration settings
    /// </summary>
    public Dictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();
        
    /// <summary>
    /// Feature flags specific to this tenant
    /// </summary>
    public Dictionary<string, bool> FeatureFlags { get; set; } = new Dictionary<string, bool>();
}