using QimErp.Shared.Common.Services.MultiTenancy;
using QimErp.Shared.Common.Services.TenantSetup;

namespace QimErp.Shared.Common.Filters;

public sealed class RequirePluginFilter(string pluginKey) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var pluginAccess = context.HttpContext.RequestServices.GetRequiredService<ITenantPluginAccessService>();
        var tenantContext = context.HttpContext.RequestServices.GetRequiredService<ITenantContext>();

        var enabled = await pluginAccess.IsPluginEnabledAsync(
            tenantContext.TenantId,
            pluginKey,
            context.HttpContext.RequestAborted);

        if (!enabled)
        {
            return Results.Json(
                new
                {
                    error = $"Plugin '{pluginKey}' is not installed for this tenant.",
                    code = "PLUGIN_NOT_INSTALLED",
                },
                statusCode: StatusCodes.Status403Forbidden);
        }

        return await next(context);
    }
}

public static class RequirePluginExtensions
{
    public static RouteHandlerBuilder RequirePlugin(this RouteHandlerBuilder builder, string pluginKey)
    {
        builder.WithMetadata(new RequirePluginAttribute(pluginKey));
        return builder.AddEndpointFilter(new RequirePluginFilter(pluginKey));
    }
}
