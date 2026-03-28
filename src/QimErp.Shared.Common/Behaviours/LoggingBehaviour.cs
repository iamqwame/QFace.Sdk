using MediatR;
using Microsoft.Extensions.Logging;
using QimErp.Shared.Common.Logging;
using QimErp.Shared.Common.Services.Auth;

namespace QimErp.Shared.Common.Behaviours;

/// <summary>
/// Wraps each MediatR request with a user/tenant logging scope and an entry log line.
/// </summary>
public sealed class LoggingBehaviour<TRequest, TResponse>(
    ILogger<LoggingBehaviour<TRequest, TResponse>> logger,
    ICurrentUserService currentUserService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        using (logger.BeginUserContextScope(currentUserService))
        {
            logger.LogInformation("Starting feature {FeatureName}", typeof(TRequest).Name);
            return await next();
        }
    }
}
