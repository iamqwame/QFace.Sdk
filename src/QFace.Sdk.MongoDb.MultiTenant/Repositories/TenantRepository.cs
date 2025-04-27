namespace QFace.Sdk.MongoDb.MultiTenant.Repositories;

/// <summary>
/// Implementation of tenant repository
/// </summary>
public class TenantRepository : MongoRepository<Tenant>, ITenantRepository
{
    /// <summary>
    /// Creates a new tenant repository
    /// </summary>
    /// <param name="database">MongoDB database</param>
    /// <param name="collectionName">Collection name</param>
    /// <param name="logger">Logger</param>
    public TenantRepository(
        IMongoDatabase database,
        string collectionName,
        ILogger<TenantRepository> logger)
        : base(database, collectionName, logger)
    {
    }
        
    /// <summary>
    /// Gets a tenant by its unique code
    /// </summary>
    public async Task<Tenant?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(code))
            return null;
                
        return await FindOneAsync(t => t.Code == code, cancellationToken: cancellationToken);
    }
        
    /// <summary>
    /// Checks if a tenant exists
    /// </summary>
    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(id))
            return false;
                
        var count = await CountAsync(t => t.Id == id, cancellationToken: cancellationToken);
        return count > 0;
    }
        
    /// <summary>
    /// Checks if a tenant with the given code exists
    /// </summary>
    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(code))
            return false;
                
        var count = await CountAsync(t => t.Code == code, cancellationToken: cancellationToken);
        return count > 0;
    }
        
    /// <summary>
    /// Updates the tenant provisioning status
    /// </summary>
    public async Task<bool> UpdateProvisioningStatusAsync(string id, bool isProvisioned, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Tenant>.Filter.Eq(t => t.Id, id);
        var update = Builders<Tenant>.Update
            .Set(t => t.IsProvisioned, isProvisioned)
            .Set(t => t.LastModifiedDate, DateTime.UtcNow);
                
        if (isProvisioned)
        {
            update = update.Set(t => t.ProvisionedDate, DateTime.UtcNow);
        }
            
        var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.ModifiedCount > 0;
    }
        
    /// <summary>
    /// Override to create custom indexes
    /// </summary>
    public override async Task CreateIndexesAsync(CancellationToken cancellationToken = default)
    {
        // Create standard indexes from base class
        await base.CreateIndexesAsync(cancellationToken);
            
        // Create unique index on Code field
        var indexModel = new CreateIndexModel<Tenant>(
            Builders<Tenant>.IndexKeys.Ascending(t => t.Code),
            new CreateIndexOptions { Unique = true, Background = true, Name = "code_unique_idx" }
        );
            
        await _collection.Indexes.CreateOneAsync(indexModel, cancellationToken: cancellationToken);
            
        // Create index on tenant type
        var typeIndexModel = new CreateIndexModel<Tenant>(
            Builders<Tenant>.IndexKeys.Ascending(t => t.TenantType),
            new CreateIndexOptions { Background = true, Name = "tenant_type_idx" }
        );
            
        await _collection.Indexes.CreateOneAsync(typeIndexModel, cancellationToken: cancellationToken);
    }
}