namespace QFace.Sdk.MongoDb.MultiTenant.Models;

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
    public AddressInfo Address { get; set; } = new AddressInfo();
}