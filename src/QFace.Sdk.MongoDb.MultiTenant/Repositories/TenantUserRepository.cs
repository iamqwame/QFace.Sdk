namespace QFace.Sdk.MongoDb.MultiTenant.Repositories
{
    /// <summary>
    /// Implementation of tenant user repository
    /// </summary>
    public class TenantUserRepository : MongoRepository<TenantUserDocument>, ITenantUserRepository
    {
        /// <summary>
        /// Creates a new tenant user repository
        /// </summary>
        /// <param name="database">MongoDB database</param>
        /// <param name="collectionName">Collection name</param>
        /// <param name="logger">Logger</param>
        public TenantUserRepository(
            IMongoDatabase database,
            string collectionName,
            ILogger<TenantUserRepository> logger)
            : base(database, collectionName, logger)
        {
        }
        
        /// <summary>
        /// Gets all tenant associations for a user
        /// </summary>
        public async Task<IEnumerable<TenantUserDocument>> GetTenantsByUserIdAsync(
            string userId, 
            bool includeInactive = false, 
            CancellationToken cancellationToken = default)
        {
            return await FindAsync(
                tu => tu.UserId == userId, 
                includeInactive, 
                cancellationToken: cancellationToken);
        }
        
        /// <summary>
        /// Gets all user associations for a tenant
        /// </summary>
        public async Task<IEnumerable<TenantUserDocument>> GetUsersByTenantIdAsync(
            string tenantId, 
            bool includeInactive = false, 
            CancellationToken cancellationToken = default)
        {
            return await FindAsync(
                tu => tu.TenantId == tenantId, 
                includeInactive, 
                cancellationToken: cancellationToken);
        }
        
        /// <summary>
        /// Gets a specific tenant-user association
        /// </summary>
        public async Task<TenantUserDocument?> GetTenantUserAsync(
            string userId, 
            string tenantId, 
            CancellationToken cancellationToken = default)
        {
            return await FindOneAsync(
                tu => tu.UserId == userId && tu.TenantId == tenantId,
                cancellationToken: cancellationToken);
        }
        
        /// <summary>
        /// Checks if a user has access to a tenant
        /// </summary>
        public async Task<bool> HasTenantAccessAsync(
            string userId, 
            string tenantId, 
            CancellationToken cancellationToken = default)
        {
            var count = await CountAsync(
                tu => tu.UserId == userId && tu.TenantId == tenantId && tu.IsActive,
                cancellationToken: cancellationToken);
                
            return count > 0;
        }
        
        /// <summary>
        /// Override to create custom indexes
        /// </summary>
        public override async Task CreateIndexesAsync(CancellationToken cancellationToken = default)
        {
            // Create standard indexes from base class
            await base.CreateIndexesAsync(cancellationToken);
            
            // Create unique index on UserId + TenantId
            var indexModel = new CreateIndexModel<TenantUserDocument>(
                Builders<TenantUserDocument>.IndexKeys
                    .Ascending(tu => tu.UserId)
                    .Ascending(tu => tu.TenantId),
                new CreateIndexOptions { Unique = true, Background = true, Name = "user_tenant_unique_idx" }
            );
            
            await _collection.Indexes.CreateOneAsync(indexModel, cancellationToken: cancellationToken);
            
            // Create index on UserId
            var userIdIndexModel = new CreateIndexModel<TenantUserDocument>(
                Builders<TenantUserDocument>.IndexKeys.Ascending(tu => tu.UserId),
                new CreateIndexOptions { Background = true, Name = "user_id_idx" }
            );
            
            await _collection.Indexes.CreateOneAsync(userIdIndexModel, cancellationToken: cancellationToken);
            
            // Create index on TenantId
            var tenantIdIndexModel = new CreateIndexModel<TenantUserDocument>(
                Builders<TenantUserDocument>.IndexKeys.Ascending(tu => tu.TenantId),
                new CreateIndexOptions { Background = true, Name = "tenant_id_idx" }
            );
            
            await _collection.Indexes.CreateOneAsync(tenantIdIndexModel, cancellationToken: cancellationToken);
        }
    }
}