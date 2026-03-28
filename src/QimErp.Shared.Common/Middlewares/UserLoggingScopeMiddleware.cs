using Microsoft.Extensions.Logging;
using QimErp.Shared.Common.Logging;
using QimErp.Shared.Common.Services.Auth;

namespace QimErp.Shared.Common.Middlewares;

/// <summary>
/// After authentication, attaches TenantId / UserEmail / UserName to the logging scope for the remainder of the request.
/// </summary>
public sealed class UserLoggingScopeMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger _logger = loggerFactory.CreateLogger("QimErp.RequestUserContext");

    public Task InvokeAsync(HttpContext context, ICurrentUserService currentUserService)
    {
        using (_logger.BeginUserContextScope(currentUserService))
            return _next(context);
    }
}

public static class UserLoggingScopeMiddlewareExtensions
{
    public static IApplicationBuilder UseUserLoggingScope(this IApplicationBuilder app) =>
        app.UseMiddleware<UserLoggingScopeMiddleware>();
}
