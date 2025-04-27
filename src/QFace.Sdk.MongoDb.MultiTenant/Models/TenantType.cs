namespace QFace.Sdk.MongoDb.MultiTenant.Models;

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