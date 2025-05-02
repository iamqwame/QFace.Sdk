namespace QFace.Sdk.MongoDb.MultiTenant.Dtos;

/// <summary>
/// Tenant summary for available tenants list
/// </summary>
public class TenantSummary
{
    /// <summary>
    /// Tenant ID
    /// </summary>
    public string Id { get; set; } = string.Empty;
        
    /// <summary>
    /// Tenant code
    /// </summary>
    public string Code { get; set; } = string.Empty;
        
    /// <summary>
    /// Tenant name
    /// </summary>
    public string Name { get; set; } = string.Empty;
        
    /// <summary>
    /// User's role in this tenant
    /// </summary>
    public string Role { get; set; } = string.Empty;
        
    /// <summary>
    /// Whether this is the user's primary tenant
    /// </summary>
    public bool IsPrimary { get; set; }
}