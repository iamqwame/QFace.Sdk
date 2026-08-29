using Microsoft.Extensions.Hosting;
using QimErp.Shared.Common.Services.MultiTenancy;
using QimErp.Shared.Common.Services.TenantSetup;

namespace QimErp.Shared.Common.Middlewares;

/// <summary>Passes when ANY of <paramref name="moduleKeys"/> is entitled.</summary>
public sealed class RequiredModuleMiddleware(
    RequestDelegate next,
    params string[] moduleKeys)
{
    public async Task InvokeAsync(
        HttpContext context,
        ITenantModuleAccessService moduleAccess,
        ITenantContext tenantContext,
        IHostEnvironment environment)
    {
        if (environment.IsEnvironment("Test")
            || IsExemptPath(context.Request.Path)
            || !(context.User.Identity?.IsAuthenticated ?? false))
        {
            await next(context);
            return;
        }

        foreach (var moduleKey in moduleKeys)
        {
            if (await moduleAccess.IsModuleEnabledAsync(tenantContext.TenantId, moduleKey, context.RequestAborted))
            {
                await next(context);
                return;
            }
        }

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsJsonAsync(new
        {
            error = moduleKeys.Length == 1
                ? $"Module '{moduleKeys[0]}' is not installed for this tenant."
                : $"None of the modules '{string.Join(", ", moduleKeys)}' are installed for this tenant.",
            code = "MODULE_NOT_INSTALLED",
        });
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
        return app.UseMiddleware<RequiredModuleMiddleware>((object)new[] { moduleKey });
    }

    public static IApplicationBuilder UseRequiredAnyModule(this IApplicationBuilder app, params string[] moduleKeys)
    {
        return app.UseMiddleware<RequiredModuleMiddleware>((object)moduleKeys);
    }
}
