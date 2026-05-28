namespace QimErp.Shared.Common.Middlewares;

/// <summary>
/// Ensures every HTTP request has a correlation id (from <c>X-Correlation-Id</c> header or generated),
/// stored in <see cref="HttpContext.Items"/> for <see cref="Services.Auth.ICurrentUserService.GetCorrelationId"/>.
/// Echoes the value on the response header for clients.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HttpContextItemKey = "QimErp.CorrelationId";
    public const string HeaderName = "X-Correlation-Id";

    public Task InvokeAsync(HttpContext context)
    {
        string correlationId;
        if (context.Request.Headers.TryGetValue(HeaderName, out var incoming) &&
            !string.IsNullOrWhiteSpace(incoming.FirstOrDefault()))
        {
            correlationId = incoming.First()!.Trim();
        }
        else
        {
            correlationId = Guid.NewGuid().ToString("N");
        }

        context.Items[HttpContextItemKey] = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        return next(context);
    }
}

public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app) =>
        app.UseMiddleware<CorrelationIdMiddleware>();
}
