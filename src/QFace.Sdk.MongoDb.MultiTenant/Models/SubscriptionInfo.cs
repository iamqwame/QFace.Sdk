namespace QFace.Sdk.MongoDb.MultiTenant.Models;

/// <summary>
/// Address information
/// </summary>
public class AddressInfo
{
    /// <summary>
    /// Street address
    /// </summary>
    public string Street { get; set; } = string.Empty;
        
    /// <summary>
    /// City
    /// </summary>
    public string City { get; set; } = string.Empty;
        
    /// <summary>
    /// State/province
    /// </summary>
    public string State { get; set; } = string.Empty;
        
    /// <summary>
    /// Postal/ZIP code
    /// </summary>
    public string PostalCode { get; set; } = string.Empty;
        
    /// <summary>
    /// Country
    /// </summary>
    public string Country { get; set; } = string.Empty;
}

/// <summary>
/// Tenant contact information
/// </summary>
public class ContactInfo
{
    /// <summary>
    /// Primary admin's name
    /// </summary>
    public string AdminName { get; set; } = string.Empty;
        
    /// <summary>
    /// Primary admin's email
    /// </summary>
    public string AdminEmail { get; set; } = string.Empty;
        
    /// <summary>
    /// Primary admin's phone
    /// </summary>
    public string AdminPhone { get; set; } = string.Empty;
        
    /// <summary>
    /// Company/organization name
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;
        
    /// <summary>
    /// Company website
    /// </summary>
    public string CompanyWebsite { get; set; } = string.Empty;
        
    /// <summary>
    /// Address information
    /// </summary>
    public AddressInfo Address { get; set; } = new();
}


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