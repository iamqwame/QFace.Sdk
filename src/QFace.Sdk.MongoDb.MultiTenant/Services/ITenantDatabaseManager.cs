namespace QFace.Sdk.MongoDb.MultiTenant.Services
{
    /// <summary>
    /// Interface for tenant database manager
    /// </summary>
    public interface ITenantDatabaseManager
    {
        /// <summary>
        /// Provisions a tenant database
        /// </summary>
        /// <param name="tenant">The tenant</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Completion task</returns>
        Task ProvisionTenantDatabaseAsync(TenantDocument tenant, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Deprovisions a tenant database
        /// </summary>
        /// <param name="tenant">The tenant</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Completion task</returns>
        Task DeprovisionTenantDatabaseAsync(TenantDocument tenant, CancellationToken cancellationToken = default);
    }
}