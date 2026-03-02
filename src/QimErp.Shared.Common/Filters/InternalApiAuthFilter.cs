using Microsoft.Extensions.Options;
using QimErp.Shared.Common.Options;

namespace QimErp.Shared.Common.Filters;

/// <summary>
/// Endpoint filter that validates X-Internal-Api-Key for requests under /internal.
/// Use with MapGroup("/internal").AddEndpointFilter&lt;InternalApiAuthFilter&gt;().
/// </summary>
public class InternalApiAuthFilter : IEndpointFilter
{
    private const string HeaderName = "X-Internal-Api-Key";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var path = context.HttpContext.Request.Path.Value ?? "";
        if (!path.StartsWith("/internal", StringComparison.OrdinalIgnoreCase))
            return await next(context);

        var options = context.HttpContext.RequestServices.GetService<IOptions<InternalApiOptions>>();
        var expectedKey = options?.Value?.ExpectedApiKey;
        if (string.IsNullOrEmpty(expectedKey))
            return await next(context);

        var providedKey = context.HttpContext.Request.Headers[HeaderName].FirstOrDefault();
        if (string.IsNullOrEmpty(providedKey) || !string.Equals(providedKey, expectedKey, StringComparison.Ordinal))
        {
            return Results.Unauthorized();
        }

        return await next(context);
    }
}
