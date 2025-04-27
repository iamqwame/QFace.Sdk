namespace QFace.Sdk.MongoDb.MultiTenant;

/// <summary>
/// Extension methods for tenant accessor
/// </summary>
public static class TenantAccessorExtensions
{
    /// <summary>
    /// Executes a function with a specific tenant context
    /// </summary>
    /// <typeparam name="T">The return type</typeparam>
    /// <param name="repository">The repository</param>
    /// <param name="tenantId">The tenant ID to use</param>
    /// <param name="func">The function to execute</param>
    /// <returns>The result of the function</returns>
    public static T WithTenant<T>(
        this IMongoRepository<BaseDocument> repository,
        string tenantId,
        Func<IMongoRepository<BaseDocument>, T> func)
    {
        if (repository == null)
            throw new ArgumentNullException(nameof(repository));
                
        if (func == null)
            throw new ArgumentNullException(nameof(func));
                
        // Get the tenant accessor from the repository if it's a tenant-aware repository
        ITenantAccessor? tenantAccessor = null;
            
        if (repository is TenantAwareRepository<TenantBaseDocument> tenantAwareRepo)
        {
            // Use reflection to get the tenant accessor field (not ideal, but works for extension method)
            var field = typeof(TenantAwareRepository<TenantBaseDocument>)
                .GetField("_tenantAccessor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
            tenantAccessor = field?.GetValue(tenantAwareRepo) as ITenantAccessor;
        }
            
        if (tenantAccessor == null)
        {
            // If we can't get the tenant accessor, just call the function directly
            return func(repository);
        }
            
        // Execute with tenant using the accessor
        return tenantAccessor.ExecuteWithTenant(tenantId, () => func(repository));
    }
        
    /// <summary>
    /// Executes an async function with a specific tenant context
    /// </summary>
    /// <typeparam name="T">The return type</typeparam>
    /// <param name="repository">The repository</param>
    /// <param name="tenantId">The tenant ID to use</param>
    /// <param name="func">The async function to execute</param>
    /// <returns>A task with the result of the function</returns>
    public static Task<T> WithTenantAsync<T>(
        this IMongoRepository<BaseDocument> repository,
        string tenantId,
        Func<IMongoRepository<BaseDocument>, Task<T>> func)
    {
        if (repository == null)
            throw new ArgumentNullException(nameof(repository));
                
        if (func == null)
            throw new ArgumentNullException(nameof(func));
                
        // Get the tenant accessor from the repository if it's a tenant-aware repository
        ITenantAccessor? tenantAccessor = null;
            
        if (repository is TenantAwareRepository<TenantBaseDocument> tenantAwareRepo)
        {
            // Use reflection to get the tenant accessor field
            var field = typeof(TenantAwareRepository<TenantBaseDocument>)
                .GetField("_tenantAccessor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
            tenantAccessor = field?.GetValue(tenantAwareRepo) as ITenantAccessor;
        }
            
        if (tenantAccessor == null)
        {
            // If we can't get the tenant accessor, just call the function directly
            return func(repository);
        }
            
        // Execute with tenant using the accessor
        return tenantAccessor.ExecuteWithTenantAsync(tenantId, () => func(repository));
    }
}