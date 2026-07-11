using QimErp.Shared.Common.Services.MultiTenancy;
using QimErp.Shared.Common.Services.TenantSetup;

namespace QimErp.Shared.Common.Middlewares;

public sealed class RequiredModuleMiddleware(
    RequestDelegate next,
    string moduleKey)
{
    public async Task InvokeAsync(
        HttpContext context,
        ITenantModuleAccessService moduleAccess,
        ITenantContext tenantContext)
    {
        if (IsExemptPath(context.Request.Path) || !(context.User.Identity?.IsAuthenticated ?? false))
        {
            await next(context);
            return;
        }

        var enabled = await moduleAccess.IsModuleEnabledAsync(
            tenantContext.TenantId,
            moduleKey,
            context.RequestAborted);

        if (!enabled)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                error = $"Module '{moduleKey}' is not installed for this tenant.",
                code = "MODULE_NOT_INSTALLED",
            });
            return;
        }

        await next(context);
    }

    private static bool IsExemptPath(PathString path)
    {
        var value = path.Value ?? string.Empty;

        return value.StartsWith("/health", StringComparison.OrdinalIgnoreCase)
               || value.StartsWith("/alive", StringComparison.OrdinalIgnoreCase)
               || value.StartsWith("/ready", StringComparison.OrdinalIgnoreCase)
               || value.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase);
    }
}

public static class RequiredModuleMiddlewareExtensions
{
    public static IApplicationBuilder UseRequiredModule(this IApplicationBuilder app, string moduleKey)
    {
        return app.UseMiddleware<RequiredModuleMiddleware>(moduleKey);
    }
}
