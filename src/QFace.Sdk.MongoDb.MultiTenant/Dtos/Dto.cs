namespace QFace.Sdk.MongoDb.MultiTenant.Dtos;

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