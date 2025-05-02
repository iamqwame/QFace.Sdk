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

/// <summary>
/// Default implementation of ITenantAccessor using AsyncLocal
/// </summary>
public class TenantAccessor : ITenantAccessor
{
    // Use AsyncLocal to ensure thread safety and async flow
    private static readonly AsyncLocal<string?> _currentTenantId = new();
        
    /// <summary>
    /// Gets the current tenant ID
    /// </summary>
    public string? GetCurrentTenantId()
    {
        return _currentTenantId.Value;
    }
        
    /// <summary>
    /// Sets the current tenant ID
    /// </summary>
    public void SetCurrentTenantId(string? tenantId)
    {
        _currentTenantId.Value = tenantId;
    }
        
    /// <summary>
    /// Clears the current tenant ID
    /// </summary>
    public void ClearCurrentTenant()
    {
        _currentTenantId.Value = null;
    }
        
    /// <summary>
    /// Temporarily changes the tenant context for the duration of the action
    /// </summary>
    public void ExecuteWithTenant(string tenantId, Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));
                
        // Save current tenant
        var originalTenant = _currentTenantId.Value;
            
        try
        {
            // Set temporary tenant
            _currentTenantId.Value = tenantId;
                
            // Execute action
            action();
        }
        finally
        {
            // Restore original tenant
            _currentTenantId.Value = originalTenant;
        }
    }
        
    /// <summary>
    /// Temporarily changes the tenant context for the duration of the function
    /// </summary>
    public T ExecuteWithTenant<T>(string tenantId, Func<T> func)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));
                
        // Save current tenant
        var originalTenant = _currentTenantId.Value;
            
        try
        {
            // Set temporary tenant
            _currentTenantId.Value = tenantId;
                
            // Execute function and return result
            return func();
        }
        finally
        {
            // Restore original tenant
            _currentTenantId.Value = originalTenant;
        }
    }
        
    /// <summary>
    /// Temporarily changes the tenant context for the duration of the async function
    /// </summary>
    public async Task<T> ExecuteWithTenantAsync<T>(string tenantId, Func<Task<T>> func)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));
                
        // Save current tenant
        var originalTenant = _currentTenantId.Value;
            
        try
        {
            // Set temporary tenant
            _currentTenantId.Value = tenantId;
                
            // Execute async function and return result
            return await func();
        }
        finally
        {
            // Restore original tenant
            _currentTenantId.Value = originalTenant;
        }
    }
        
    /// <summary>
    /// Temporarily changes the tenant context for the duration of the async action
    /// </summary>
    public async Task ExecuteWithTenantAsync(string tenantId, Func<Task> func)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));
                
        // Save current tenant
        var originalTenant = _currentTenantId.Value;
            
        try
        {
            // Set temporary tenant
            _currentTenantId.Value = tenantId;
                
            // Execute async action
            await func();
        }
        finally
        {
            // Restore original tenant
            _currentTenantId.Value = originalTenant;
        }
    }
}