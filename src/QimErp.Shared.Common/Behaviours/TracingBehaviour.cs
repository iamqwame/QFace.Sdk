using System.Diagnostics;

namespace QimErp.Shared.Common.Behaviours;

/// <summary>
/// Creates an OpenTelemetry <see cref="Activity"/> for each MediatR request. Register after <see cref="LoggingBehaviour{TRequest,TResponse}"/>.
/// Add <c>AddSource("QimErp.MediatR")</c> (or your host application name) when configuring tracing.
/// </summary>
public sealed class TracingBehaviour<TRequest, TResponse>(
    ILogger<TracingBehaviour<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly ActivitySource Source = new(ObservabilityTelemetry.MediatRActivitySourceName);

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var operationName = typeof(TRequest).Name;

        using var activity = Source.StartActivity(operationName);
        activity?.SetTag("mediator.request", operationName);

        try
        {
            var response = await next();
            activity?.SetStatus(ActivityStatusCode.Ok);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "MediatR tracing: error in {RequestType}", operationName);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("exception.type", ex.GetType().FullName);
            activity?.SetTag("exception.message", ex.Message);
            throw;
        }
    }
}
