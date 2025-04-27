namespace QFace.Sdk.MongoDb.MultiTenant.Core;

/// <summary>
/// Default implementation of ITenantAccessor using AsyncLocal
/// </summary>
public class TenantAccessor : ITenantAccessor
{
    // Use AsyncLocal to ensure thread safety and async flow
    private static readonly AsyncLocal<string?> _currentTenantId = new AsyncLocal<string?>();
        
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