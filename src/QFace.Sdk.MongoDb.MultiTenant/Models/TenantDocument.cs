namespace QFace.Sdk.MongoDb.MultiTenant.Models;

/// <summary>
/// Tenant document for storing tenant configuration
/// </summary>
public class Tenant : BaseDocument
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
    public TenantType TenantType { get; set; } = TenantType.Shared;
        
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
    public SubscriptionInfo Subscription { get; set; } = new();
        
    /// <summary>
    /// Contact information
    /// </summary>
    public ContactInfo Contact { get; set; } = new();
        
    /// <summary>
    /// Tenant configuration settings
    /// </summary>
    public Dictionary<string, string> Settings { get; set; } = new();
        
    /// <summary>
    /// Feature flags specific to this tenant
    /// </summary>
    public Dictionary<string, bool> FeatureFlags { get; set; } = new();
}

/// <summary>
/// Tenant types
/// </summary>
public enum TenantType
{
    /// <summary>
    /// Default tenant type with dedicated database
    /// </summary>
    Dedicated,
        
    /// <summary>
    /// Shared tenant with data in shared database
    /// </summary>
    Shared,
        
    /// <summary>
    /// System tenant (special privileges)
    /// </summary>
    System
}


/// <summary>
/// Tenant user mapping document (for multi-tenant user access control)
/// </summary>
public class TenantUser : TenantBaseDocument
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
    public List<string> Permissions { get; set; } = new();
        
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
    
    
    
    // New authentication-related properties
        
        /// <summary>
        /// Last successful login date for this user in this tenant
        /// </summary>
        public DateTime? LastLoginDate { get; set; }
        
        /// <summary>
        /// Count of failed login attempts since last successful login
        /// </summary>
        public int FailedLoginAttempts { get; set; } = 0;
        
        /// <summary>
        /// Whether the account is locked due to too many failed attempts
        /// </summary>
        public bool IsLocked { get; set; } = false;
        
        /// <summary>
        /// When the account lock expires (if temporary)
        /// </summary>
        public DateTime? LockExpiryDate { get; set; }
        
        /// <summary>
        /// Whether this user requires multi-factor authentication for this tenant
        /// </summary>
        public bool RequiresMfa { get; set; } = false;
        
        /// <summary>
        /// Whether the user has completed MFA setup for this tenant
        /// </summary>
        public bool MfaConfigured { get; set; } = false;
        
        /// <summary>
        /// Secret key for MFA (encrypted)
        /// </summary>
        public string? MfaSecretKey { get; set; }
        
        /// <summary>
        /// Custom settings for this user in this tenant
        /// </summary>
        public Dictionary<string, string> UserSettings { get; set; } = new();
        
        /// <summary>
        /// When the user's access to this tenant expires (if temporary)
        /// </summary>
        public DateTime? AccessExpiryDate { get; set; }
        
        /// <summary>
        /// Whether the user has accepted the tenant's terms of service
        /// </summary>
        public bool HasAcceptedTerms { get; set; } = false;
        
        /// <summary>
        /// When the user accepted the terms of service
        /// </summary>
        public DateTime? TermsAcceptedDate { get; set; }
        
        /// <summary>
        /// Version of the terms that were accepted
        /// </summary>
        public string? AcceptedTermsVersion { get; set; }
        
        /// <summary>
        /// Checks if the user account is currently locked
        /// </summary>
        public bool IsCurrentlyLocked()
        {
            if (!IsLocked) return false;
            if (!LockExpiryDate.HasValue) return true;
            return LockExpiryDate.Value > DateTime.UtcNow;
        }
        
        /// <summary>
        /// Checks if the user's access has expired
        /// </summary>
        public bool HasAccessExpired()
        {
            if (!AccessExpiryDate.HasValue) return false;
            return AccessExpiryDate.Value < DateTime.UtcNow;
        }
        
        /// <summary>
        /// Records a successful login
        /// </summary>
        public void RecordSuccessfulLogin()
        {
            LastLoginDate = DateTime.UtcNow;
            FailedLoginAttempts = 0;
            IsLocked = false;
            LockExpiryDate = null;
        }
        
        /// <summary>
        /// Records a failed login attempt, potentially locking the account
        /// </summary>
        /// <param name="maxAttempts">Maximum number of failed attempts before locking</param>
        /// <param name="lockoutMinutes">Minutes to lock the account for</param>
        /// <returns>Whether the account is now locked</returns>
        public bool RecordFailedLogin(int maxAttempts = 5, int lockoutMinutes = 15)
        {
            FailedLoginAttempts++;
            
            if (FailedLoginAttempts >= maxAttempts)
            {
                IsLocked = true;
                LockExpiryDate = DateTime.UtcNow.AddMinutes(lockoutMinutes);
                return true;
            }
            
            return false;
        }
}