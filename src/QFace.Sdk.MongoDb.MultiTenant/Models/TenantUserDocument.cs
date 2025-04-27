namespace QFace.Sdk.MongoDb.MultiTenant.Models;

/// <summary>
/// Tenant user mapping document (for multi-tenant user access control)
/// </summary>
public class TenantUserDocument : TenantBaseDocument
{
    /// <summary>
    /// User ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;

        
    /// <summary>
    /// User's role within this tenant
    /// </summary>
    public string Role { get; set; } = "User";
        
    /// <summary>
    /// Additional tenant-specific permissions
    /// </summary>
    public List<string> Permissions { get; set; } = new List<string>();
        
    /// <summary>
    /// When user was added to tenant
    /// </summary>
    public DateTime AddedDate { get; set; } = DateTime.UtcNow;
        
    /// <summary>
    /// Who added this user to the tenant
    /// </summary>
    public string AddedBy { get; set; } = string.Empty;
        
    /// <summary>
    /// Whether this is the user's primary tenant
    /// </summary>
    public bool IsPrimaryTenant { get; set; }
}