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
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (tenantAccessor == null) throw new ArgumentNullException(nameof(tenantAccessor));
            if (tenantService == null) throw new ArgumentNullException(nameof(tenantService));

            _logger.LogInformation("Starting tenant resolution for request: {Path}", context.Request.Path);

            if (ShouldSkipTenantResolution(context))
            {
                _logger.LogInformation("Skipping tenant resolution for excluded path: {Path}", context.Request.Path);

                tenantAccessor.ClearCurrentTenant();
                await _next(context);
                return;
            }

            try
            {
                string? tenantId = null;
                string? tenantCode = null;

                if (_options.UseRouteResolution)
                {
                    (tenantId, tenantCode) = TryResolveFromRoute(context);
                    _logger.LogDebug("Route resolution attempt - TenantId: {TenantId}, TenantCode: {TenantCode}", tenantId, tenantCode);
                }

                if (string.IsNullOrEmpty(tenantId) && _options.UseHeaderResolution)
                {
                    (tenantId, tenantCode) = TryResolveFromHeader(context);
                    _logger.LogDebug("Header resolution attempt - TenantId: {TenantId}, TenantCode: {TenantCode}", tenantId, tenantCode);
                }

                if (string.IsNullOrEmpty(tenantId) && _options.UseQueryStringResolution)
                {
                    (tenantId, tenantCode) = TryResolveFromQueryString(context);
                    _logger.LogDebug("Query string resolution attempt - TenantId: {TenantId}, TenantCode: {TenantCode}", tenantId, tenantCode);
                }

                if (string.IsNullOrEmpty(tenantId) && _options.UseCookieResolution)
                {
                    (tenantId, tenantCode) = TryResolveFromCookie(context);
                    _logger.LogDebug("Cookie resolution attempt - TenantId: {TenantId}, TenantCode: {TenantCode}", tenantId, tenantCode);
                }

                if (string.IsNullOrEmpty(tenantId) && _options.UseAuthClaimResolution)
                {
                    (tenantId, tenantCode) = TryResolveFromClaims(context);
                    _logger.LogDebug("Auth claims resolution attempt - TenantId: {TenantId}, TenantCode: {TenantCode}", tenantId, tenantCode);
                }

                if (string.IsNullOrEmpty(tenantId) && !string.IsNullOrEmpty(tenantCode))
                {
                    _logger.LogInformation("Attempting to resolve tenantId from tenantCode: {TenantCode}", tenantCode);
                    var tenant = await tenantService.GetByCodeAsync(tenantCode);
                    tenantId = tenant?.Id;
                    if (tenant == null)
                    {
                        _logger.LogWarning("No tenant found for tenantCode: {TenantCode}", tenantCode);
                    }
                }

                if (!string.IsNullOrEmpty(tenantId))
                {
                    var tenant = await tenantService.GetByIdAsync(tenantId);

                    if (tenant != null && tenant.IsActive)
                    {
                        if (tenant.IsProvisioned)
                        {
                            _logger.LogInformation("Successfully resolved tenant: {TenantId} - {TenantCode}", tenant.Id, tenant.Code);

                            tenantAccessor.SetCurrentTenantId(tenantId);

                            if (_options.IncludeTenantInfoInResponse)
                            {
                                context.Response.Headers["X-Tenant-ID"] = tenantId;
                                if (!string.IsNullOrEmpty(tenant.Code))
                                {
                                    context.Response.Headers["X-Tenant-Code"] = tenant.Code;
                                }
                            }
                        }
                        else if (_options.RejectUnprovisionedTenants)
                        {
                            _logger.LogWarning("Tenant is not provisioned: {TenantId}", tenantId);
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            await context.Response.WriteAsync("Tenant database has not been provisioned");
                            return;
                        }
                    }
                    else if (_options.RejectInvalidTenants)
                    {
                        _logger.LogWarning("Tenant invalid or inactive: {TenantId}", tenantId);
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsync("Invalid or inactive tenant");
                        return;
                    }
                }
                else if (_options.RequireTenant && !IsExemptFromTenantRequirement(context))
                {
                    _logger.LogWarning("Tenant resolution failed and tenant is required for path: {Path}", context.Request.Path);
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Tenant identifier is required");
                    return;
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception during tenant resolution");
                if (_options.FailOnResolutionError)
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsync("Error resolving tenant");
                }
                else
                {
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
        
        if (context.User.Identity?.IsAuthenticated == true)
        {
            // Try to get tenant ID from various possible claim names
            foreach (var claimName in _options.TenantIdClaimNames)
            {
                var claim = context.User.FindFirst(claimName);
                if (claim != null && !string.IsNullOrEmpty(claim.Value))
                {
                    tenantId = claim.Value;
                    _logger.LogDebug("Found tenant ID in claim '{ClaimName}': {TenantId}", claimName, tenantId);
                    break;
                }
            }
        
            // Try to get tenant code from various possible claim names
            foreach (var claimName in _options.TenantCodeClaimNames)
            {
                var claim = context.User.FindFirst(claimName);
                if (claim != null && !string.IsNullOrEmpty(claim.Value))
                {
                    tenantCode = claim.Value;
                    _logger.LogDebug("Found tenant code in claim '{ClaimName}': {TenantCode}", claimName, tenantCode);
                    break;
                }
            }
        }
    
        if (!string.IsNullOrEmpty(tenantId) || !string.IsNullOrEmpty(tenantCode))
        {
            _logger.LogInformation("Resolved tenant from claims - ID: {TenantId}, Code: {TenantCode}", 
                tenantId ?? "(not found)", tenantCode ?? "(not found)");
        }
        else
        {
            _logger.LogDebug("No tenant information found in claims");
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