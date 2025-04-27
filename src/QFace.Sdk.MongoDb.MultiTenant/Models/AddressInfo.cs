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