namespace QFace.Sdk.MongoDb.MultiTenant.Repositories;

/// <summary>
/// Interface for tenant user repository
/// </summary>
public interface ITenantUserRepository : IMongoRepository<TenantUser>
{
    /// <summary>
    /// Gets all tenant associations for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="includeInactive">Whether to include inactive associations</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The tenant associations</returns>
    Task<IEnumerable<TenantUser>> GetTenantsByUserIdAsync(
        string userId, 
        bool includeInactive = false, 
        CancellationToken cancellationToken = default);
        
    /// <summary>
    /// Gets all user associations for a tenant
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <param name="includeInactive">Whether to include inactive associations</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user associations</returns>
    Task<IEnumerable<TenantUser>> GetUsersByTenantIdAsync(
        string tenantId, 
        bool includeInactive = false, 
        CancellationToken cancellationToken = default);
        
    /// <summary>
    /// Gets a specific tenant-user association
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="tenantId">The tenant ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The tenant-user association or null if not found</returns>
    Task<TenantUser?> GetTenantUserAsync(
        string userId, 
        string tenantId, 
        CancellationToken cancellationToken = default);
        
    /// <summary>
    /// Checks if a user has access to a tenant
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="tenantId">The tenant ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the user has access, false otherwise</returns>
    Task<bool> HasTenantAccessAsync(
        string userId, 
        string tenantId, 
        CancellationToken cancellationToken = default);
}