namespace QFace.Sdk.MongoDb.MultiTenant.Dtos;

/// <summary>
/// Authentication result with tokens and user information
/// </summary>
public class AuthResult
{
    /// <summary>
    /// Whether authentication was successful
    /// </summary>
    public bool IsSuccess { get; set; }
        
    /// <summary>
    /// Error message if authentication failed
    /// </summary>
    public string? ErrorMessage { get; set; }
        
    /// <summary>
    /// User ID if authenticated
    /// </summary>
    public string? UserId { get; set; }
        
    /// <summary>
    /// User's full name
    /// </summary>
    public string? FullName { get; set; }
        
    /// <summary>
    /// User's email
    /// </summary>
    public string? Email { get; set; }
        
    /// <summary>
    /// Tenant ID the user is authenticated for
    /// </summary>
    public string? TenantId { get; set; }
        
    /// <summary>
    /// Tenant code
    /// </summary>
    public string? TenantCode { get; set; }
        
    /// <summary>
    /// Tenant name
    /// </summary>
    public string? TenantName { get; set; }
        
    /// <summary>
    /// JWT access token
    /// </summary>
    public string? AccessToken { get; set; }
        
    /// <summary>
    /// When the access token expires
    /// </summary>
    public DateTime? AccessTokenExpiresAt { get; set; }
        
    /// <summary>
    /// Refresh token
    /// </summary>
    public string? RefreshToken { get; set; }
        
    /// <summary>
    /// User's role in the tenant
    /// </summary>
    public string? Role { get; set; }
        
    /// <summary>
    /// User's permissions in the tenant
    /// </summary>
    public List<string> Permissions { get; set; } = new();
        
    /// <summary>
    /// Whether MFA is required
    /// </summary>
    public bool RequiresMfa { get; set; }
        
    /// <summary>
    /// Whether MFA verification has been completed
    /// </summary>
    public bool MfaVerified { get; set; }
        
    /// <summary>
    /// Other tenants the user has access to
    /// </summary>
    public List<TenantSummary> AvailableTenants { get; set; } = new();
        
    /// <summary>
    /// Create a failed authentication result
    /// </summary>
    public static AuthResult Failed(string errorMessage)
    {
        return new AuthResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}