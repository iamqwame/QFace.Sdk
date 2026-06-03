using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Temporalio.Activities;
using Temporalio.Worker.Interceptors;
using Temporalio.Worker;

namespace QFace.Sdk.Temporal.Interceptors;

/// <summary>
/// Minimal interface that any ambient identity service (e.g. UserContextService) can implement
/// to receive tenant context from the Temporal activity interceptor.
/// Defined here so QFace.Sdk.Temporal has no dependency on QimErp.Shared.Common.
/// </summary>
public interface ITenantContextSetter
{
    void SetTenantContext(string tenantId, string userEmail, string? userName = null, string? userId = null);
    void ClearTenantContext();
}

/// <summary>
/// Temporal worker interceptor that automatically seeds <see cref="ICurrentUserService"/>
/// with the TenantId (and optional UserId) from every activity's input payload before
/// the activity body executes.
///
/// This means <see cref="QimErp.Shared.Common.Interceptors.AuditEntitySaveChangesInterceptor"/>
/// always sees a non-empty TenantId — no per-entity <c>WithTenantId()</c> calls needed anywhere.
///
/// Convention: the activity input type must expose a public <c>TenantId</c> string/Guid property,
/// or optionally a <c>UserId</c> / <c>UserEmail</c> property. If absent the interceptor
/// logs a warning but does NOT throw — the activity runs, and any DB saves will hit the
/// interceptor's null-guard and throw with a clear message.
///
/// Register once in <c>BuildWorker()</c>:
/// <code>
///   workerOpts.Interceptors = [new TenantContextActivityInterceptor()];
/// </code>
/// </summary>
/// <summary>
/// Worker-level interceptor. Receives <see cref="IServiceScopeFactory"/> at construction so it
/// can resolve <see cref="ITenantContextSetter"/> per-activity without coupling to a request scope.
/// Because <c>UserContextService</c> stores tenant identity in a <c>static AsyncLocal</c>,
/// the value set here flows automatically to every other DI scope on the same async call chain —
/// including the Temporalio activity scope where EF Core operations run.
/// </summary>
public sealed class TenantContextActivityInterceptor(IServiceScopeFactory scopeFactory) : IWorkerInterceptor
{
    public ActivityInboundInterceptor InterceptActivity(ActivityInboundInterceptor nextInterceptor)
        => new TenantSeedingInboundInterceptor(nextInterceptor, scopeFactory);

    // Workflows do not touch EF Core — pass through unchanged.
    public WorkflowInboundInterceptor InterceptWorkflow(WorkflowInboundInterceptor nextInterceptor)
        => nextInterceptor;

    // ── Inner interceptor ──────────────────────────────────────────────────────

    private sealed class TenantSeedingInboundInterceptor(
        ActivityInboundInterceptor next,
        IServiceScopeFactory scopeFactory)
        : ActivityInboundInterceptor(next)
    {
        public override async Task<object?> ExecuteActivityAsync(ExecuteActivityInput input)
        {
            // ActivityExecutionContext.Current throws outside a real Temporal worker (e.g. in tests).
            // Access it safely — we only need it for the activity type name in log messages.
            var activityType = TryGetActivityType();

            var userSvc   = TryResolveService();
            var tenantId  = ExtractProperty(input.Args, "TenantId");
            var userId    = ExtractProperty(input.Args, "UserId") ?? "temporal-system";
            var userEmail = ExtractProperty(input.Args, "UserEmail") ?? "temporal@system";
            var userName  = ExtractProperty(input.Args, "UserName") ?? "Temporal Worker";

            var logger = TryGetLogger();

            if (userSvc is null)
            {
                logger?.LogWarning(
                    "[TenantContextActivityInterceptor] ITenantContextSetter not resolved for activity {ActivityType}. " +
                    "TenantId will not be auto-stamped on DB entities.",
                    activityType);
                return await base.ExecuteActivityAsync(input);
            }

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                logger?.LogWarning(
                    "[TenantContextActivityInterceptor] No TenantId found on input for activity {ActivityType}. " +
                    "DB saves will throw at the interceptor level if context is not seeded elsewhere.",
                    activityType);
                return await base.ExecuteActivityAsync(input);
            }

            userSvc.SetTenantContext(tenantId, userEmail, userName, userId);
            logger?.LogDebug(
                "[TenantContextActivityInterceptor] Set ambient TenantId={TenantId} for activity {ActivityType}",
                tenantId, activityType);
            try
            {
                return await base.ExecuteActivityAsync(input);
            }
            finally
            {
                userSvc.ClearTenantContext();
            }
        }

        private static string TryGetActivityType()
        {
            try { return ActivityExecutionContext.Current.Info.ActivityType; }
            catch { return "<unknown>"; }
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private ITenantContextSetter? TryResolveService()
        {
            try
            {
                // Create a short-lived scope just to resolve the setter.
                // The static AsyncLocal in UserContextService means the value set here
                // flows to the Temporalio activity scope on the same async chain.
                using var scope = scopeFactory.CreateScope();
                return scope.ServiceProvider.GetService<ITenantContextSetter>();
            }
            catch { return null; }
        }

        private ILogger? TryGetLogger()
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                return scope.ServiceProvider.GetService<ILoggerFactory>()
                    ?.CreateLogger<TenantContextActivityInterceptor>();
            }
            catch { return null; }
        }

        /// <summary>
        /// Extracts a named string/Guid property from the first activity argument using reflection.
        /// Returns null if not found.
        /// </summary>
        private static string? ExtractProperty(object[] args, string propertyName)
        {
            foreach (var arg in args)
            {
                if (arg is null) continue;
                var prop = arg.GetType().GetProperty(propertyName);
                if (prop is null) continue;
                var value = prop.GetValue(arg);
                return value?.ToString();
            }
            return null;
        }
    }
}
