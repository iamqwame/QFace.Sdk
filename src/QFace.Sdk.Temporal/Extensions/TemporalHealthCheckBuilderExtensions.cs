using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using QFace.Sdk.Temporal.HealthChecks;

namespace QFace.Sdk.Temporal.Extensions;

public static class TemporalHealthCheckBuilderExtensions
{
    /// <summary>
    /// Adds a Temporal connectivity health check using GetSystemInfoAsync.
    /// Reports Unhealthy when Temporal is unreachable — stops readiness probe
    /// so load balancers and orchestration platforms pull the instance from rotation.
    ///
    /// Tag defaults to "ready" so it appears on /ready but not /alive.
    /// Liveness (/alive) should not depend on external service connectivity.
    ///
    /// Usage:
    ///   builder.Services.AddHealthChecks()
    ///       .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
    ///       .AddTemporalHealthCheck();
    /// </summary>
    public static IHealthChecksBuilder AddTemporalHealthCheck(
        this IHealthChecksBuilder builder,
        string name = "temporal",
        HealthStatus failureStatus = HealthStatus.Unhealthy,
        IEnumerable<string>? tags = null)
    {
        tags ??= ["ready"];

        builder.AddCheck<TemporalHealthCheck>(
            name,
            failureStatus,
            tags);

        return builder;
    }
}
