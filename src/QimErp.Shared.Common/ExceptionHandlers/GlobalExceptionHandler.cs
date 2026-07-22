using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace QimErp.Shared.Common.ExceptionHandlers;

/// <summary>
/// Shared global exception handler — logs the unhandled exception with endpoint + tenant
/// context and writes a ProblemDetails response. Registered once per WebApi
/// project via <c>builder.Services.AddExceptionHandler&lt;GlobalExceptionHandler&gt;()</c> so
/// feature handlers can let exceptions bubble instead of repeating a local
/// <c>catch (Exception) { logger.LogError(...); return Result.WithFailure(...); }</c> block
/// (Invariant 7). Any <see cref="ILogger.BeginScope"/> the handler set before throwing
/// carries through to this log call automatically (TenantId / EmployeeId / etc.).
///
/// <see cref="ValidationPipelineBehavior{TRequest,TResponse}"/> throws
/// <see cref="ValidationException"/> for command/query validation failures that a handler
/// never gets a chance to translate via the normal <c>Result</c> pattern — those are a client
/// input error (400), not a server fault, so they're special-cased here instead of falling
/// through to the generic 500 below.
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

        if (exception is ValidationException validationException)
        {
            logger.LogWarning(
                "Validation failure in {Endpoint} (TenantId={TenantId}): {Errors}",
                endpoint, tenantId, validationException.Message);

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(new ValidationProblemDetails(
                validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()))
            {
                Title = "Validation failed",
                Status = StatusCodes.Status400BadRequest,
                Instance = httpContext.Request.Path,
            }, cancellationToken);

            return true;
        }

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
