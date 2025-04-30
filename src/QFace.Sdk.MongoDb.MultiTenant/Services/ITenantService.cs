using QFace.Sdk.MongoDb.MultiTenant.Dtos;

namespace QFace.Sdk.MongoDb.MultiTenant.Services;

/// <summary>
/// Interface for tenant service
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Gets a tenant by ID
    /// </summary>
    /// <param name="id">The tenant ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The tenant or null if not found</returns>
    Task<Tenant?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        
    /// <summary>
    /// Gets a tenant by code
    /// </summary>
    /// <param name="code">The tenant code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The tenant or null if not found</returns>
    Task<Tenant?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
        
    /// <summary>
    /// Gets all tenants
    /// </summary>
    /// <param name="includeInactive">Whether to include inactive tenants</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>All tenants</returns>
    Task<IEnumerable<Tenant>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
        
    /// <summary>
    /// Creates a new tenant
    /// </summary>
    /// <param name="tenant">The tenant to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The ID of the created tenant</returns>
    Task<string> CreateAsync(Tenant tenant, CancellationToken cancellationToken = default);

    Task<TenantCreationResult> CreateTenantAsync(
        TenantCreationRequest request,
        CancellationToken cancellationToken = default);
        
    /// <summary>
    /// Updates a tenant
    /// </summary>
    /// <param name="tenant">The tenant to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default);
        
    /// <summary>
    /// Deletes a tenant
    /// </summary>
    /// <param name="id">The tenant ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        
    /// <summary>
    /// Provisions a tenant database
    /// </summary>
    /// <param name="id">The tenant ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> ProvisionAsync(string id, CancellationToken cancellationToken = default);
        
    /// <summary>
    /// Gets tenants accessible by a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="includeInactive">Whether to include inactive tenants</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The accessible tenants</returns>
    Task<IEnumerable<Tenant>> GetAccessibleTenantsAsync(
        string userId, 
        bool includeInactive = false, 
        CancellationToken cancellationToken = default);
        
    /// <summary>
    /// Validates tenant access for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="tenantId">The tenant ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if access is valid, false otherwise</returns>
    Task<bool> ValidateTenantAccessAsync(
        string userId, 
        string tenantId, 
        CancellationToken cancellationToken = default);
}