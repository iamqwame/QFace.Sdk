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
    
    /// <summary>
    /// Request model for creating a new tenant with admin user
    /// </summary>
    public class TenantCreationRequest
    {
        /// <summary>
        /// Basic tenant information
        /// </summary>
        public TenantInfo TenantInfo { get; set; } = new();
        
        /// <summary>
        /// Tenant admin user information
        /// </summary>
        public TenantAdminInfo AdminInfo { get; set; } = new();
        
        /// <summary>
        /// Advanced tenant configuration (optional)
        /// </summary>
        public TenantConfigurationInfo? Configuration { get; set; }
    }
    
    /// <summary>
    /// Basic tenant information
    /// </summary>
    public class TenantInfo
    {
        /// <summary>
        /// Tenant code (unique identifier, used in URLs)
        /// If not provided, will be generated from name
        /// </summary>
        public string? Code { get; set; }
        
        /// <summary>
        /// Display name of the tenant
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Optional description
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Type of tenant
        /// </summary>
        public TenantType TenantType { get; set; } = TenantType.Shared;
        
        /// <summary>
        /// Contact information
        /// </summary>
        public ContactInfo? Contact { get; set; }
        
        /// <summary>
        /// Subscription information
        /// </summary>
        public SubscriptionInfo? Subscription { get; set; }
    }
    
    /// <summary>
    /// Tenant admin user information
    /// </summary>
    public class TenantAdminInfo
    {
        /// <summary>
        /// Admin user's email
        /// </summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Admin user's full name
        /// </summary>
        public string FullName { get; set; } = string.Empty;
        
        /// <summary>
        /// Admin user's initial password
        /// </summary>
        public string Password { get; set; } = string.Empty;
        
        /// <summary>
        /// Optional username (if different from email)
        /// </summary>
        public string? Username { get; set; }
        
        /// <summary>
        /// Whether to send welcome email
        /// </summary>
        public bool SendWelcomeEmail { get; set; } = true;
    }
    
    /// <summary>
    /// Advanced tenant configuration options
    /// </summary>
    public class TenantConfigurationInfo
    {
        /// <summary>
        /// Whether to provision the database immediately
        /// </summary>
        public bool ProvisionImmediately { get; set; } = true;
        
        /// <summary>
        /// Custom settings
        /// </summary>
        public Dictionary<string, string> Settings { get; set; } = new();
        
        /// <summary>
        /// Feature flags
        /// </summary>
        public Dictionary<string, bool> FeatureFlags { get; set; } = new();
        
        /// <summary>
        /// Custom database name (if not using default naming)
        /// </summary>
        public string? CustomDatabaseName { get; set; }
        
        /// <summary>
        /// Custom connection string (if using dedicated database server)
        /// </summary>
        public string? CustomConnectionString { get; set; }
    }
    
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