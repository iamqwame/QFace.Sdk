using QimErp.Shared.Common.Services.MultiTenancy;

namespace QimErp.Shared.Common.Middlewares;

public class TenantContextMiddleware
{
    private readonly RequestDelegate _next;

    public TenantContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext, ICurrentUserService currentUserService)
    {
        var tenantId = NormalizeTenantId(currentUserService.GetTenantId());
        tenantContext.SetTenant(tenantId);
        
        await _next(context);
    }

    /// <summary>
    /// Aligns JWT/header tenant ids with DB values: Guid claims may differ only by casing, but PostgreSQL
    /// uses case-sensitive string equality, which breaks EF global filters (e.TenantId == context).
    /// </summary>
    internal static string? NormalizeTenantId(string? tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            return tenantId;

        return Guid.TryParse(tenantId.Trim(), out var g) ? g.ToString("D") : tenantId.Trim();
    }
}

public static class TenantContextMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantContext(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TenantContextMiddleware>();
    }
}

