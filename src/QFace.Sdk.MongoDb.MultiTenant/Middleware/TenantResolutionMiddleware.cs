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

                var (claimTenantId, claimTenantCode) = _options.UseAuthClaimResolution
                    ? TryResolveFromClaims(context)
                    : (null, null);
                var hasClaimTenant = !string.IsNullOrEmpty(claimTenantId) || !string.IsNullOrEmpty(claimTenantCode);

                if (hasClaimTenant)
                {
                    tenantId = await ResolveTenantIdAsync(claimTenantId, claimTenantCode, tenantService);

                    foreach (var (hintId, hintCode) in GetEnabledHints(context))
                    {
                        if (string.IsNullOrEmpty(hintId) && string.IsNullOrEmpty(hintCode))
                            continue;

                        var hintTenantId = await ResolveTenantIdAsync(hintId, hintCode, tenantService);
                        if (!await HintCodeMatchesIdAsync(hintId, hintCode, tenantService)
                            || !string.Equals(hintTenantId, tenantId, StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogWarning("Tenant hint does not match authenticated tenant for path: {Path}", context.Request.Path);
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            await context.Response.WriteAsync("Tenant mismatch");
                            return;
                        }
                    }
                }
                else if (_options.AllowUnauthenticatedTenantHints)
                {
                    foreach (var (hintId, hintCode) in GetEnabledHints(context))
                    {
                        if (string.IsNullOrEmpty(hintId) && string.IsNullOrEmpty(hintCode))
                            continue;

                        tenantId = await ResolveTenantIdAsync(hintId, hintCode, tenantService);
                        if (!await HintCodeMatchesIdAsync(hintId, hintCode, tenantService))
                        {
                            _logger.LogWarning("Tenant id and code hints disagree for path: {Path}", context.Request.Path);
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            await context.Response.WriteAsync("Tenant mismatch");
                            return;
                        }

                        break;
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
                    if (context.User.Identity?.IsAuthenticated != true)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Authentication required");
                        return;
                    }

                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Tenant identifier is required");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception during tenant resolution");
                if (_options.FailOnResolutionError && !context.Response.HasStarted)
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsync("Error resolving tenant");
                    return;
                }

                throw;
            }

            await _next(context);
        }

    private IEnumerable<(string? TenantId, string? TenantCode)> GetEnabledHints(HttpContext context)
    {
        if (_options.UseRouteResolution)
            yield return TryResolveFromRoute(context);
        if (_options.UseHeaderResolution)
            yield return TryResolveFromHeader(context);
        if (_options.UseQueryStringResolution)
            yield return TryResolveFromQueryString(context);
        if (_options.UseCookieResolution)
            yield return TryResolveFromCookie(context);
    }

    private static async Task<bool> HintCodeMatchesIdAsync(string? tenantId, string? tenantCode, ITenantService tenantService)
    {
        if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(tenantCode))
            return true;

        var tenant = await tenantService.GetByCodeAsync(tenantCode);
        return tenant != null && string.Equals(tenant.Id, tenantId, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<string?> ResolveTenantIdAsync(string? tenantId, string? tenantCode, ITenantService tenantService)
    {
        if (!string.IsNullOrEmpty(tenantId))
            return tenantId;

        if (string.IsNullOrEmpty(tenantCode))
            return null;

        var tenant = await tenantService.GetByCodeAsync(tenantCode);
        return tenant?.Id;
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
        foreach (var excludedPath in _options.ExcludedPaths)
        {
            if (context.Request.Path.StartsWithSegments(excludedPath, StringComparison.OrdinalIgnoreCase))
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
        foreach (var exemptPath in _options.TenantExemptPaths)
        {
            if (context.Request.Path.StartsWithSegments(exemptPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
            
        return false;
    }
}