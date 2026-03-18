using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Temporalio.Client;

namespace QFace.Sdk.Temporal.HealthChecks;

/// <summary>
/// Health check that verifies the Temporal server is reachable.
/// Reports Unhealthy when the Temporal connection is down so load balancers
/// and orchestration platforms (Railway, k8s) stop routing traffic.
///
/// Registered via: builder.Services.AddHealthChecks().AddTemporalHealthCheck()
/// Expose on /ready endpoint (tag: "ready") so liveness (/alive) is unaffected.
/// </summary>
public sealed class TemporalHealthCheck(
    ITemporalClient client,
    ILogger<TemporalHealthCheck> logger) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // GetSystemInfoAsync is the lightest available RPC — no workflow state involved.
            // It verifies the connection, authentication, and namespace access in one call.
            await client.Connection.WorkflowService.GetSystemInfoAsync(
                new Temporalio.Api.WorkflowService.V1.GetSystemInfoRequest(),
                new RpcOptions { CancellationToken = cancellationToken });

            return HealthCheckResult.Healthy("Temporal connection is healthy.");
        }
        catch (OperationCanceledException)
        {
            return HealthCheckResult.Degraded("Temporal health check timed out.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[TemporalHealthCheck] Temporal connection check failed.");
            return HealthCheckResult.Unhealthy(
                $"Temporal connection failed: {ex.Message}", ex);
        }
    }
}
