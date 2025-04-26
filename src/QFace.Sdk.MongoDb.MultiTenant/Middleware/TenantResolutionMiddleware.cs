namespace QFace.Sdk.MongoDb.MultiTenant.Middleware;

/// <summary>
/// Middleware that resolves current tenant from request
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly TenantResolutionOptions _options;
    private readonly ILogger<TenantResolutionMiddleware> _logger;
        
    /// <summary>
    /// Creates a new tenant resolution middleware
    /// </summary>
    /// <param name="next">The next middleware in the pipeline</param>
    /// <param name="options">Options for tenant resolution</param>
    /// <param name="logger">The logger</param>
    public TenantResolutionMiddleware(
        RequestDelegate next, 
        TenantResolutionOptions options,
        ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _options = options ?? new TenantResolutionOptions();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
        
    /// <summary>
    /// Processes a request to resolve tenant
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <param name="tenantAccessor">The tenant accessor</param>
    /// <param name="tenantService">The tenant service</param>
    public async Task InvokeAsync(
        HttpContext context,
        ITenantAccessor tenantAccessor,
        ITenantService tenantService)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));
                
        if (tenantAccessor == null)
            throw new ArgumentNullException(nameof(tenantAccessor));
                
        if (tenantService == null)
            throw new ArgumentNullException(nameof(tenantService));
                
        // Skip tenant resolution for excluded paths
        if (ShouldSkipTenantResolution(context))
        {
            // Clear tenant ID to ensure no tenant context
            tenantAccessor.ClearCurrentTenant();
                
            // Continue with request pipeline
            await _next(context);
            return;
        }
            
        try
        {
            // Resolve tenant ID from request
            string? tenantId = null;
            string? tenantCode = null;
                
            if (_options.UseRouteResolution)
            {
                (tenantId, tenantCode) = TryResolveFromRoute(context);
            }
                
            if (string.IsNullOrEmpty(tenantId) && _options.UseHeaderResolution)
            {
                (tenantId, tenantCode) = TryResolveFromHeader(context);
            }
                
            if (string.IsNullOrEmpty(tenantId) && _options.UseQueryStringResolution)
            {
                (tenantId, tenantCode) = TryResolveFromQueryString(context);
            }
                
            if (string.IsNullOrEmpty(tenantId) && _options.UseCookieResolution)
            {
                (tenantId, tenantCode) = TryResolveFromCookie(context);
            }
                
            if (string.IsNullOrEmpty(tenantId) && _options.UseAuthClaimResolution)
            {
                (tenantId, tenantCode) = TryResolveFromClaims(context);
            }
                
            // If we have a tenant code but no ID, try to resolve ID from code
            if (string.IsNullOrEmpty(tenantId) && !string.IsNullOrEmpty(tenantCode))
            {
                var tenant = await tenantService.GetByCodeAsync(tenantCode);
                tenantId = tenant?.Id;
            }
                
            // If we have a tenant ID, verify and set it
            if (!string.IsNullOrEmpty(tenantId))
            {
                var tenant = await tenantService.GetByIdAsync(tenantId);
                    
                if (tenant != null && tenant.IsActive)
                {
                    if (tenant.IsProvisioned)
                    {
                        // Set tenant ID in accessor
                        tenantAccessor.SetCurrentTenantId(tenantId);
                            
                        // Add tenant info to response headers if configured
                        if (_options.IncludeTenantInfoInResponse)
                        {
                            context.Response.Headers["X-Tenant-ID"] = tenantId;
                            if (!string.IsNullOrEmpty(tenant.Code))
                            {
                                context.Response.Headers["X-Tenant-Code"] = tenant.Code;
                            }
                        }
                            
                        _logger.LogDebug("Resolved tenant ID: {TenantId}, Code: {TenantCode}", 
                            tenantId, tenant.Code);
                    }
                    else if (_options.RejectUnprovisionedTenants)
                    {
                        _logger.LogWarning("Rejected request for unprovisioned tenant: {TenantId}", tenantId);
                            
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsync("Tenant database has not been provisioned");
                        return;
                    }
                }
                else if (_options.RejectInvalidTenants)
                {
                    _logger.LogWarning("Rejected request for invalid or inactive tenant: {TenantId}", tenantId);
                        
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("Invalid or inactive tenant");
                    return;
                }
            }
            else if (_options.RequireTenant && !IsExemptFromTenantRequirement(context))
            {
                _logger.LogWarning("Rejected request with no tenant identifier");
                    
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Tenant identifier is required");
                return;
            }
                
            // Continue with request pipeline
            await _next(context);
        }
        catch (Exception ex)
        {
            // Handle exceptions during tenant resolution
            _logger.LogError(ex, "Error during tenant resolution");
                
            if (_options.FailOnResolutionError)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("Error resolving tenant");
            }
            else
            {
                // Continue with request pipeline
                await _next(context);
            }
        }
    }
        
    /// <summary>
    /// Tries to resolve tenant from route values
    /// </summary>
    private (string? TenantId, string? TenantCode) TryResolveFromRoute(HttpContext context)
    {
        string? tenantId = null;
        string? tenantCode = null;
            
        if (context.Request.RouteValues.TryGetValue("tenantId", out var routeTenantId))
        {
            tenantId = routeTenantId?.ToString();
        }
            
        if (context.Request.RouteValues.TryGetValue("tenantCode", out var routeTenantCode))
        {
            tenantCode = routeTenantCode?.ToString();
        }
            
        return (tenantId, tenantCode);
    }
        
    /// <summary>
    /// Tries to resolve tenant from headers
    /// </summary>
    private (string? TenantId, string? TenantCode) TryResolveFromHeader(HttpContext context)
    {
        string? tenantId = null;
        string? tenantCode = null;
            
        if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var headerTenantId))
        {
            tenantId = headerTenantId.ToString();
        }
            
        if (context.Request.Headers.TryGetValue("X-Tenant-Code", out var headerTenantCode))
        {
            tenantCode = headerTenantCode.ToString();
        }
            
        return (tenantId, tenantCode);
    }
        
    /// <summary>
    /// Tries to resolve tenant from query string
    /// </summary>
    private (string? TenantId, string? TenantCode) TryResolveFromQueryString(HttpContext context)
    {
        string? tenantId = null;
        string? tenantCode = null;
            
        if (context.Request.Query.TryGetValue("tenantId", out var queryTenantId))
        {
            tenantId = queryTenantId.ToString();
        }
            
        if (context.Request.Query.TryGetValue("tenantCode", out var queryTenantCode))
        {
            tenantCode = queryTenantCode.ToString();
        }
            
        return (tenantId, tenantCode);
    }
        
    /// <summary>
    /// Tries to resolve tenant from cookies
    /// </summary>
    private (string? TenantId, string? TenantCode) TryResolveFromCookie(HttpContext context)
    {
        string? tenantId = null;
        string? tenantCode = null;
            
        if (context.Request.Cookies.TryGetValue("tenant_id", out var cookieTenantId))
        {
            tenantId = cookieTenantId;
        }
            
        if (context.Request.Cookies.TryGetValue("tenant_code", out var cookieTenantCode))
        {
            tenantCode = cookieTenantCode;
        }
            
        return (tenantId, tenantCode);
    }
        
    /// <summary>
    /// Tries to resolve tenant from claims
    /// </summary>
    private (string? TenantId, string? TenantCode) TryResolveFromClaims(HttpContext context)
    {
        string? tenantId = null;
        string? tenantCode = null;
            
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = context.User.FindFirst("tenant_id");
            if (tenantIdClaim != null)
            {
                tenantId = tenantIdClaim.Value;
            }
                
            var tenantCodeClaim = context.User.FindFirst("tenant_code");
            if (tenantCodeClaim != null)
            {
                tenantCode = tenantCodeClaim.Value;
            }
        }
            
        return (tenantId, tenantCode);
    }
        
    /// <summary>
    /// Checks if tenant resolution should be skipped for the current request
    /// </summary>
    private bool ShouldSkipTenantResolution(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";
            
        // Check if path matches any excluded path
        foreach (var excludedPath in _options.ExcludedPaths)
        {
            if (path.StartsWith(excludedPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
            
        return false;
    }
        
    /// <summary>
    /// Checks if a request is exempt from tenant requirement
    /// </summary>
    private bool IsExemptFromTenantRequirement(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";
            
        // Check if path matches any tenant-exempt path
        foreach (var exemptPath in _options.TenantExemptPaths)
        {
            if (path.StartsWith(exemptPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
            
        return false;
    }
}