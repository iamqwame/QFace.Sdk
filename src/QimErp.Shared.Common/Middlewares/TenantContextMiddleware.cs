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
        var tenantId = currentUserService.GetTenantId();
        tenantContext.SetTenant(tenantId);
        
        await _next(context);
    }
}

public static class TenantContextMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantContext(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TenantContextMiddleware>();
    }
}

