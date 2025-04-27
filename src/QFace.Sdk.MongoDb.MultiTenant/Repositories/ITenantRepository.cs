namespace QFace.Sdk.MongoDb.MultiTenant.Repositories;

/// <summary>
/// Interface for tenant repository
/// </summary>
public interface ITenantRepository : IMongoRepository<Tenant>
{
    /// <summary>
    /// Gets a tenant by its unique code
    /// </summary>
    /// <param name="code">The tenant code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The tenant or null if not found</returns>
    Task<Tenant?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
        
    /// <summary>
    /// Checks if a tenant exists
    /// </summary>
    /// <param name="id">The tenant ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the tenant exists, false otherwise</returns>
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);
        
    /// <summary>
    /// Checks if a tenant with the given code exists
    /// </summary>
    /// <param name="code">The tenant code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if a tenant with the code exists, false otherwise</returns>
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);
        
    /// <summary>
    /// Updates the tenant provisioning status
    /// </summary>
    /// <param name="id">The tenant ID</param>
    /// <param name="isProvisioned">Whether the tenant is provisioned</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> UpdateProvisioningStatusAsync(string id, bool isProvisioned, CancellationToken cancellationToken = default);
}