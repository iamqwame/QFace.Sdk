using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace QimErp.Shared.Common.ExceptionHandlers;

/// <summary>
/// Shared global exception handler — logs the unhandled exception with endpoint + tenant
/// context and writes a generic ProblemDetails 500 response. Registered once per WebApi
/// project via <c>builder.Services.AddExceptionHandler&lt;GlobalExceptionHandler&gt;()</c> so
/// feature handlers can let exceptions bubble instead of repeating a local
/// <c>catch (Exception) { logger.LogError(...); return Result.WithFailure(...); }</c> block
/// (Invariant 7). Any <see cref="ILogger.BeginScope"/> the handler set before throwing
/// carries through to this log call automatically (TenantId / EmployeeId / etc.).
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var endpoint = httpContext.GetEndpoint()?.DisplayName ?? httpContext.Request.Path.Value ?? "(unknown)";
        var tenantId = httpContext.User.FindFirst("tenant_id")?.Value;

        logger.LogError(
            exception,
            "Unhandled exception in {Endpoint} (TenantId={TenantId})",
            endpoint, tenantId);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Title = "An unexpected error occurred",
            Detail = "The request could not be completed. Please try again later.",
            Status = StatusCodes.Status500InternalServerError,
            Instance = httpContext.Request.Path,
        }, cancellationToken);

        return true;
    }
}
