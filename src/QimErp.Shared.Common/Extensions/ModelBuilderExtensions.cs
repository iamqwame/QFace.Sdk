using QimErp.Shared.Common.Services.MultiTenancy;

namespace QimErp.Shared.Common.Extensions;

/// <summary>
/// Implemented by the DbContext so the global query filter can read the current tenant id through a
/// DbContext-rooted member access. See <see cref="ModelBuilderExtensions.ApplyGlobalFilter{TEntity}"/>
/// for why this must be the DbContext and not the injected <see cref="ITenantContext"/> service.
/// </summary>
public interface ITenantQueryFilterContext
{
    string? CurrentTenantId { get; }
}

public static class ModelBuilderExtensions
{
    public static void ApplyGlobalFilters(this ModelBuilder modelBuilder, ITenantQueryFilterContext context)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType))
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
        modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
            e.DataStatus != DataState.Deleted &&
            (e.IsGlobal || context.CurrentTenantId == null || context.CurrentTenantId == "" || e.TenantId == context.CurrentTenantId)
        );
    }
}
