namespace QFace.Sdk.MongoDb.MultiTenant.Models;

public class TenantBaseDocument:BaseDocument
{
    /// <summary>
    /// Identifier for the tenant that owns this document (for multi-tenancy)
    /// This will be empty/null if multi-tenancy is not being used
    /// </summary>
    public string TenantId { get; set; }
}