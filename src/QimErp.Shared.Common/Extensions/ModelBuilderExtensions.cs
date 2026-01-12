using QimErp.Shared.Common.Services.MultiTenancy;

namespace QimErp.Shared.Common.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyGlobalFilters(this ModelBuilder modelBuilder, ITenantContext tenantContext)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            typeof(ModelBuilderExtensions)
                .GetMethod(nameof(ApplyGlobalFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(entityType.ClrType)
                .Invoke(null, [modelBuilder, tenantContext]);
        }
    }

    private static void ApplyGlobalFilter<TEntity>(
        ModelBuilder modelBuilder,
        ITenantContext tenantContext)
        where TEntity : AuditableEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
            e.DataStatus != DataState.Deleted &&
            (e.IsGlobal || string.IsNullOrEmpty(tenantContext.TenantId) || e.TenantId == tenantContext.TenantId)
        );
    }
}
