using QimErp.Shared.Common.Services.MultiTenancy;

namespace QimErp.Shared.Common.Extensions;

/// <summary>
/// Implemented by the DbContext so the global query filter can read the current tenant id through a
/// DbContext-rooted member access. See <see cref="ModelBuilderExtensions.ApplyGlobalFilter{TEntity}"/>
/// for why this must be the DbContext and not the injected <see cref="ITenantContext"/> service.
/// <para>The value is NON-NULL by contract: when no tenant is in scope the implementer returns
/// <see cref="ModelBuilderExtensions.NoTenantSentinel"/>, which no real row can equal — so the
/// filter FAILS CLOSED (returns zero tenant rows) instead of fail-open (returning every tenant).</para>
/// </summary>
public interface ITenantQueryFilterContext
{
    string CurrentTenantId { get; }
}

/// <summary>
/// Implemented by the DbContext, not a service — same funcletization reason documented on
/// <see cref="ITenantQueryFilterContext"/>. <see cref="AllowedCompanyIds"/> always contains
/// <c>""</c> when <see cref="CompanyFilterActive"/>, folded in at the context layer so the
/// predicate stays a single <c>= ANY(...)</c> instead of an OR the planner cannot index.
/// </summary>
public interface ICompanyQueryFilterContext
{
    bool CompanyFilterActive { get; }
    string[] AllowedCompanyIds { get; }
}

/// <summary>
/// Exists so the 17 Accounting configurations that already take <see cref="ITenantQueryFilterContext"/>
/// can gain company scoping by changing one word, with no call-site churn.
/// </summary>
public interface IScopedQueryFilterContext : ITenantQueryFilterContext, ICompanyQueryFilterContext
{
}

public static class ModelBuilderExtensions
{
    /// <summary>
    /// Filter key used when no tenant is in ambient scope. The spaces and colons mean it can never
    /// collide with a real GUID-format ("D") TenantId; a query with no tenant therefore matches only
    /// <c>IsGlobal</c> rows (fail-closed). Genuine cross-tenant reads must opt out explicitly via
    /// <c>IgnoreQueryFilters()</c> (and, once enabled, are still bounded by Postgres RLS).
    /// </summary>
    public const string NoTenantSentinel = " ::no-tenant::";

    /// <summary>
    /// Superseded by <c>TenantCompanyQueryFilterConvention</c>, which runs at model finalizing and so
    /// also reaches entity types that have no <c>DbSet&lt;&gt;</c>. Calling this remains safe: the
    /// convention ANDs the company clause onto whatever filter it finds.
    /// </summary>
    [Obsolete("Filters are applied by TenantCompanyQueryFilterConvention; this call is redundant.")]
    public static void ApplyGlobalFilters(this ModelBuilder modelBuilder, ITenantQueryFilterContext context)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            if (entityType.IsOwned() || entityType.BaseType is not null || entityType.FindPrimaryKey() is null)
                continue;

            typeof(ModelBuilderExtensions)
                .GetMethod(nameof(ApplyGlobalFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(entityType.ClrType)
                .Invoke(null, [modelBuilder, context]);
        }
    }

    private static void ApplyGlobalFilter<TEntity>(
        ModelBuilder modelBuilder,
        ITenantQueryFilterContext context)
        where TEntity : AuditableEntity
    {
        // `context` is the DbContext instance (it implements ITenantQueryFilterContext). EF Core
        // recognizes member access rooted on the running DbContext and emits a query parameter that
        // it re-reads on every execution — so one cached compiled plan is reused safely across
        // tenants. Closing over the injected ITenantContext service here instead made EF funcletize
        // the value into a baked-in SQL constant, which poisoned the plan cache across tenants until
        // a process restart (the post-onboarding empty-read anomaly).
        //
        // FAIL-CLOSED: CurrentTenantId is never null (no tenant in scope → NoTenantSentinel), so this
        // is a plain `TenantId = @p OR IsGlobal` disjunction — sargable (BitmapOr over a TenantId index
        // and an IsGlobal partial index) and matching ZERO tenant rows when context is absent, instead
        // of the previous `@p IS NULL ⇒ all tenants` fail-open hole. Cross-tenant reads opt out via
        // IgnoreQueryFilters(); Postgres RLS enforces the boundary underneath regardless.
        modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
            e.DataStatus != DataState.Deleted &&
            (e.IsGlobal || e.TenantId == context.CurrentTenantId)
        );
    }
}
