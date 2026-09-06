using System.Diagnostics;
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
/// Minimal interface that sets the ambient <c>ITenantContext</c> used by EF Core global query filters.
/// Defined here so QFace.Sdk.Temporal has no dependency on QimErp.Shared.Common.
/// Implemented by <c>TenantContext</c> in QimErp.Shared.Common.
/// </summary>
public interface ITenantScopeSetter
{
    void SetTenantScope(string? tenantId);
    void ClearTenantScope();
}

/// <summary>
/// Minimal interface that sets the ambient <c>ICompanyContext</c> used by EF Core company query
/// filters and by the company stamp in the audit interceptor.
/// Defined here so QFace.Sdk.Temporal has no dependency on QimErp.Shared.Common.
/// Implemented by <c>CompanyContext</c> in QimErp.Shared.Common.
/// </summary>
public interface ICompanyScopeSetter
{
    void SetCompanyScope(string? activeCompanyId, IReadOnlyCollection<string>? allowedCompanyIds, bool filterActive);
    void ClearCompanyScope();
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

            var userSvc      = TryResolveService();
            var tenantScope  = TryResolveTenantScope();
            var companyScope = TryResolveCompanyScope();
            var tenantId     = ExtractProperty(input.Args, "TenantId");
            var companyId    = ExtractTopLevelProperty(input.Args, "CompanyId");
            var userId       = ExtractProperty(input.Args, "UserId") ?? "temporal-system";
            var userEmail    = ExtractProperty(input.Args, "UserEmail") ?? "temporal@system";
            var userName     = ExtractProperty(input.Args, "UserName") ?? "Temporal Worker";

            var logger = TryGetLogger();

            // Every log line for the rest of this activity carries TenantId + TraceId, so
            // downstream logs (including ones emitted deep inside the activity body) stay
            // correlated to the workflow's distributed trace instead of going dark at the
            // Temporal boundary.
            using var scope = logger?.BeginScope(BuildScopeItems(tenantId, companyId, activityType));

            if (userSvc is null)
            {
                logger?.LogWarning(
                    "ITenantContextSetter not resolved for activity {ActivityType}. " +
                    "TenantId will not be auto-stamped on DB entities.",
                    activityType);
                // Do NOT return early — ITenantScopeSetter must still be seeded so EF global
                // query filters (HasQueryFilter) return the correct tenant's rows.
            }

            // Absent CompanyId reproduces the pre-multi-company behaviour: no company filter,
            // rows stamped tenant-shared.
            if (string.IsNullOrWhiteSpace(companyId))
                companyScope?.SetCompanyScope(null, null, filterActive: false);
            else
                companyScope?.SetCompanyScope(companyId, [companyId], filterActive: true);

            try
            {
                if (string.IsNullOrWhiteSpace(tenantId))
                {
                    logger?.LogWarning(
                        "No TenantId found on input for activity {ActivityType}. " +
                        "DB saves will throw at the interceptor level if context is not seeded elsewhere.",
                        activityType);
                    return await base.ExecuteActivityAsync(input);
                }

                userSvc?.SetTenantContext(tenantId, userEmail, userName, userId);
                // Seed ITenantContext so EF global query filters (HasQueryFilter) see the correct
                // tenant and do not return cross-tenant rows or fail the filter check.
                tenantScope?.SetTenantScope(tenantId);
                logger?.LogDebug("Set ambient TenantId={TenantId} CompanyId={CompanyId} for activity {ActivityType}",
                    tenantId, companyId, activityType);

                return await base.ExecuteActivityAsync(input);
            }
            finally
            {
                userSvc?.ClearTenantContext();
                tenantScope?.ClearTenantScope();
                companyScope?.ClearCompanyScope();
            }
        }

        private static List<KeyValuePair<string, object>> BuildScopeItems(string? tenantId, string? companyId, string activityType)
        {
            var items = new List<KeyValuePair<string, object>>(4) { new("ActivityType", activityType) };
            if (!string.IsNullOrWhiteSpace(tenantId))
                items.Add(new("TenantId", tenantId));
            if (!string.IsNullOrWhiteSpace(companyId))
                items.Add(new("CompanyId", companyId));
            var traceId = Activity.Current?.TraceId.ToString();
            if (!string.IsNullOrWhiteSpace(traceId))
                items.Add(new("TraceId", traceId));
            return items;
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

        private ITenantScopeSetter? TryResolveTenantScope()
        {
            try
            {
                // Resolves ITenantScopeSetter (implemented by TenantContext) to seed the
                // ambient ITenantContext used by EF global query filters.
                // Uses the same AsyncLocal pattern as ITenantContextSetter — value set here
                // flows to the Temporalio activity scope on the same async chain.
                using var scope = scopeFactory.CreateScope();
                return scope.ServiceProvider.GetService<ITenantScopeSetter>();
            }
            catch { return null; }
        }

        private ICompanyScopeSetter? TryResolveCompanyScope()
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                return scope.ServiceProvider.GetService<ICompanyScopeSetter>();
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
        /// Bulk activities receive bare collections (e.g. List&lt;EmployeeChangedEvent&gt;) with no
        /// top-level TenantId, so collection args fall back to probing their first element —
        /// every item in a bulk batch belongs to the same tenant by construction.
        /// Returns null if not found.
        /// </summary>
        private static string? ExtractProperty(object?[] args, string propertyName)
        {
            foreach (var arg in args)
            {
                if (arg is null) continue;

                var direct = ExtractFromInstance(arg, propertyName);
                if (direct is not null) return direct;

                if (arg is System.Collections.IEnumerable enumerable and not string)
                {
                    foreach (var item in enumerable)
                    {
                        if (item is null) continue;
                        var fromItem = ExtractFromInstance(item, propertyName);
                        if (fromItem is not null) return fromItem;
                        break;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Extracts a named property from the top level of an activity argument only.
        /// Never probes a collection's first element: a bulk batch is single-tenant by construction
        /// but NOT single-company, so probing would stamp a whole cross-company batch with one
        /// company's id.
        /// </summary>
        private static string? ExtractTopLevelProperty(object?[] args, string propertyName)
        {
            foreach (var arg in args)
            {
                if (arg is null) continue;

                var direct = ExtractFromInstance(arg, propertyName);
                if (direct is not null) return direct;
            }
            return null;
        }

        private static string? ExtractFromInstance(object instance, string propertyName)
        {
            var prop = instance.GetType().GetProperty(propertyName);
            if (prop is null) return null;
            var value = prop.GetValue(instance)?.ToString();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }
}
