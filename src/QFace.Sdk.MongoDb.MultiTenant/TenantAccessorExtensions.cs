namespace QFace.Sdk.MongoDb.MultiTenant;

/// <summary>
/// Extension methods for tenant accessor that make it easier to work with tenant contexts
/// </summary>
public static class TenantAccessorExtensions
{
    /// <summary>
    /// Executes code as a specific tenant, then restores the original tenant context
    /// </summary>
    /// <param name="tenantAccessor">The tenant accessor</param>
    /// <param name="tenantId">The tenant ID to use temporarily</param>
    /// <param name="action">The action to execute as the tenant</param>
    public static void AsTenant(this ITenantAccessor tenantAccessor, string tenantId, Action action)
    {
        tenantAccessor.ExecuteWithTenant(tenantId, action);
    }

    /// <summary>
    /// Executes code as a specific tenant with a return value, then restores the original tenant context
    /// </summary>
    /// <typeparam name="T">The return type</typeparam>
    /// <param name="tenantAccessor">The tenant accessor</param>
    /// <param name="tenantId">The tenant ID to use temporarily</param>
    /// <param name="func">The function to execute as the tenant</param>
    /// <returns>The result of the function</returns>
    public static T AsTenant<T>(this ITenantAccessor tenantAccessor, string tenantId, Func<T> func)
    {
        return tenantAccessor.ExecuteWithTenant(tenantId, func);
    }

    /// <summary>
    /// Executes async code as a specific tenant, then restores the original tenant context
    /// </summary>
    /// <param name="tenantAccessor">The tenant accessor</param>
    /// <param name="tenantId">The tenant ID to use temporarily</param>
    /// <param name="func">The async function to execute as the tenant</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public static Task AsTenantAsync(this ITenantAccessor tenantAccessor, string tenantId, Func<Task> func)
    {
        return tenantAccessor.ExecuteWithTenantAsync(tenantId, func);
    }

    /// <summary>
    /// Executes async code as a specific tenant with a return value, then restores the original tenant context
    /// </summary>
    /// <typeparam name="T">The return type</typeparam>
    /// <param name="tenantAccessor">The tenant accessor</param>
    /// <param name="tenantId">The tenant ID to use temporarily</param>
    /// <param name="func">The async function to execute as the tenant</param>
    /// <returns>A task with the result of the function</returns>
    public static Task<T> AsTenantAsync<T>(this ITenantAccessor tenantAccessor, string tenantId, Func<Task<T>> func)
    {
        return tenantAccessor.ExecuteWithTenantAsync(tenantId, func);
    }
}