using QimErp.Shared.Common.Logging;

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
        var requestTypeName = typeof(TRequest).Name;
        using (logger.BeginMediatrObservabilityScope(currentUserService, requestTypeName))
        {
            if (requestTypeName.EndsWith("Query", StringComparison.Ordinal))
                logger.LogDebug("Starting feature {FeatureName}", requestTypeName);
            else
                logger.LogInformation("Starting feature {FeatureName}", requestTypeName);

            return await next();
        }
    }
}
