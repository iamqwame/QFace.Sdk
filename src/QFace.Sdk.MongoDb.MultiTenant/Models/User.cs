namespace QFace.Sdk.MongoDb.MultiTenant.Models;

/// <summary>
    /// User document for storing user information
    /// </summary>
    public class User : BaseDocument
    {
        /// <summary>
        /// User's email address (unique)
        /// </summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// User's username (unique, if different from email)
        /// </summary>
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// User's full name
        /// </summary>
        public string FullName { get; set; } = string.Empty;
        
        /// <summary>
        /// User's password hash
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;
        
        /// <summary>
        /// User's password salt
        /// </summary>
        public string PasswordSalt { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether the user's email is verified
        /// </summary>
        public bool IsEmailVerified { get; set; } = false;
        
        /// <summary>
        /// Whether the user is a system administrator
        /// </summary>
        public bool IsSystemAdmin { get; set; } = false;
        
        /// <summary>
        /// When the user's password was last changed
        /// </summary>
        public DateTime? LastPasswordChangeDate { get; set; }
        
        /// <summary>
        /// Phone number
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether the phone number is verified
        /// </summary>
        public bool IsPhoneVerified { get; set; } = false;
        
        /// <summary>
        /// User profile picture URL or path
        /// </summary>
        public string? ProfilePictureUrl { get; set; }
        
        /// <summary>
        /// Custom user preferences
        /// </summary>
        public Dictionary<string, string> Preferences { get; set; } = new();
        
        /// <summary>
        /// User metadata (for extensibility)
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
    
    
    // <summary>
    /// Refresh token document for token persistence
    /// </summary>
    public class RefreshToken : TenantBaseDocument
    {
        /// <summary>
        /// The token value
        /// </summary>
        public string Token { get; set; } = string.Empty;
        
        /// <summary>
        /// User ID the token belongs to
        /// </summary>
        public string UserId { get; set; } = string.Empty;
        
        /// <summary>
        /// When the token expires
        /// </summary>
        public DateTime ExpiresAt { get; set; }
        
        /// <summary>
        /// Client application that requested the token
        /// </summary>
        public string ClientId { get; set; } = string.Empty;
        
        /// <summary>
        /// Device information
        /// </summary>
        public string DeviceInfo { get; set; } = string.Empty;
        
        /// <summary>
        /// IP address the token was issued to
        /// </summary>
        public string IpAddress { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether the token has been revoked
        /// </summary>
        public bool IsRevoked { get; set; } = false;
        
        /// <summary>
        /// When the token was revoked
        /// </summary>
        public DateTime? RevokedAt { get; set; }
        
        /// <summary>
        /// Reason the token was revoked
        /// </summary>
        public string? RevocationReason { get; set; }
        
        /// <summary>
        /// Replacement token if this one was superseded
        /// </summary>
        public string? ReplacedByToken { get; set; }
        
        /// <summary>
        /// Checks if the token is active
        /// </summary>
        public bool IsActive => !IsRevoked && ExpiresAt > DateTime.UtcNow;
    }