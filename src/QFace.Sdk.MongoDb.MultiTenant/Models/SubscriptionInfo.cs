namespace QFace.Sdk.MongoDb.MultiTenant.Models;

/// <summary>
/// Tenant subscription information
/// </summary>
public class SubscriptionInfo
{
    /// <summary>
    /// Subscription tier/plan
    /// </summary>
    public string Tier { get; set; } = "Free";
        
    /// <summary>
    /// When the subscription expires
    /// </summary>
    public DateTime? ExpiryDate { get; set; }
        
    /// <summary>
    /// Whether this is a trial account
    /// </summary>
    public bool IsTrialAccount { get; set; }
        
    /// <summary>
    /// Maximum number of users allowed
    /// </summary>
    public int? MaxUsers { get; set; }
        
    /// <summary>
    /// Maximum storage space in MB
    /// </summary>
    public long? MaxStorageMB { get; set; }
        
    /// <summary>
    /// When this subscription was created
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
    /// <summary>
    /// When this subscription was last renewed
    /// </summary>
    public DateTime? LastRenewedDate { get; set; }
}