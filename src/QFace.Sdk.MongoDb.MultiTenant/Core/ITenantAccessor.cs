namespace QFace.Sdk.MongoDb.MultiTenant.Core;

/// <summary>
/// Interface for accessing the current tenant context
/// </summary>
public interface ITenantAccessor
{
    /// <summary>
    /// Gets the current tenant ID
    /// </summary>
    /// <returns>The current tenant ID or null if not set</returns>
    string? GetCurrentTenantId();
        
    /// <summary>
    /// Sets the current tenant ID
    /// </summary>
    /// <param name="tenantId">The tenant ID to set</param>
    void SetCurrentTenantId(string? tenantId);
        
    /// <summary>
    /// Clears the current tenant ID
    /// </summary>
    void ClearCurrentTenant();
        
    /// <summary>
    /// Temporarily changes the tenant context for the duration of the action
    /// </summary>
    /// <param name="tenantId">The tenant ID to use</param>
    /// <param name="action">The action to execute</param>
    void ExecuteWithTenant(string tenantId, Action action);
        
    /// <summary>
    /// Temporarily changes the tenant context for the duration of the function
    /// </summary>
    /// <typeparam name="T">The return type</typeparam>
    /// <param name="tenantId">The tenant ID to use</param>
    /// <param name="func">The function to execute</param>
    /// <returns>The result of the function</returns>
    T ExecuteWithTenant<T>(string tenantId, Func<T> func);
        
    /// <summary>
    /// Temporarily changes the tenant context for the duration of the async function
    /// </summary>
    /// <typeparam name="T">The return type</typeparam>
    /// <param name="tenantId">The tenant ID to use</param>
    /// <param name="func">The async function to execute</param>
    /// <returns>A task with the result of the function</returns>
    Task<T> ExecuteWithTenantAsync<T>(string tenantId, Func<Task<T>> func);
        
    /// <summary>
    /// Temporarily changes the tenant context for the duration of the async action
    /// </summary>
    /// <param name="tenantId">The tenant ID to use</param>
    /// <param name="func">The async action to execute</param>
    /// <returns>A task representing the completion of the action</returns>
    Task ExecuteWithTenantAsync(string tenantId, Func<Task> func);
}