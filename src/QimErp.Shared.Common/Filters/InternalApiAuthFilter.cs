using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using QimErp.Shared.Common.Options;

namespace QimErp.Shared.Common.Filters;

/// <summary>
/// Endpoint filter that validates X-Internal-Api-Key on every endpoint it is attached to.
/// Fails closed: an unset <see cref="InternalApiOptions.ExpectedApiKey"/> rejects all requests.
/// Use with MapGroup("/internal").AddEndpointFilter&lt;InternalApiAuthFilter&gt;().
/// </summary>
public class InternalApiAuthFilter(IOptions<InternalApiOptions> options) : IEndpointFilter
{
    private const string HeaderName = "X-Internal-Api-Key";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var expectedKey = options.Value.ExpectedApiKey;
        if (string.IsNullOrEmpty(expectedKey))
            return Results.Unauthorized();

        var providedKey = context.HttpContext.Request.Headers[HeaderName].FirstOrDefault();
        if (string.IsNullOrEmpty(providedKey) || !FixedTimeEquals(providedKey, expectedKey))
            return Results.Unauthorized();

        return await next(context);
    }

    private static bool FixedTimeEquals(string provided, string expected) =>
        CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(provided),
            Encoding.UTF8.GetBytes(expected));
}
