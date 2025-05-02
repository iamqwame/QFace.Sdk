namespace QFace.Sdk.MongoDb.MultiTenant.Dtos;

/// <summary>
/// Result of tenant creation operation
/// </summary>
public class TenantCreationResult
{
    /// <summary>
    /// Whether the creation was successful
    /// </summary>
    public bool Success { get; set; }
        
    /// <summary>
    /// Error message if creation failed
    /// </summary>
    public string? ErrorMessage { get; set; }
        
    /// <summary>
    /// ID of the created tenant
    /// </summary>
    public string? TenantId { get; set; }
        
    /// <summary>
    /// Code of the created tenant
    /// </summary>
    public string? TenantCode { get; set; }
        
    /// <summary>
    /// Name of the created tenant
    /// </summary>
    public string? TenantName { get; set; }
        
    /// <summary>
    /// ID of the admin user created
    /// </summary>
    public string? AdminUserId { get; set; }
        
    /// <summary>
    /// Email of the admin user
    /// </summary>
    public string? AdminEmail { get; set; }
        
    /// <summary>
    /// Whether the tenant was provisioned
    /// </summary>
    public bool IsProvisioned { get; set; }
        
    /// <summary>
    /// Creates a failed result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <returns>Failed result</returns>
    public static TenantCreationResult Failed(string errorMessage)
    {
        return new TenantCreationResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}