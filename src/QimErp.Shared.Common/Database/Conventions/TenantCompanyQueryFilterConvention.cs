using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace QimErp.Shared.Common.Database.Conventions;

/// <summary>
/// Applies the tenant + company global query filter at <c>ModelFinalizing</c>, i.e. after every module
/// has run <c>ApplyConfigurationsFromAssembly</c>, so entity types reachable only through an
/// <see cref="IEntityTypeConfiguration{TEntity}"/> (no <c>DbSet&lt;&gt;</c>) are filtered too.
/// </summary>
public sealed class TenantCompanyQueryFilterConvention(IScopedQueryFilterContext context)
    : IModelFinalizingConvention
{
    private static readonly MethodInfo FullFilterBuilder =
        typeof(TenantCompanyQueryFilterConvention).GetMethod(nameof(BuildFullFilter), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly MethodInfo TenantFilterBuilder =
        typeof(TenantCompanyQueryFilterConvention).GetMethod(nameof(BuildTenantFilter), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly MethodInfo CompanyClauseBuilder =
        typeof(TenantCompanyQueryFilterConvention).GetMethod(nameof(BuildCompanyClause), BindingFlags.NonPublic | BindingFlags.Static)!;

    public void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> conventionContext)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            if (!typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            if (entityType.IsOwned() || entityType.BaseType is not null || entityType.FindPrimaryKey() is null)
                continue;

            var tenantWide = typeof(ITenantWideEntity).IsAssignableFrom(entityType.ClrType);
            var existing = entityType.GetQueryFilter();

            if (existing is null)
            {
                var builder = tenantWide ? TenantFilterBuilder : FullFilterBuilder;
                SetFilter(entityType, (LambdaExpression)builder.MakeGenericMethod(entityType.ClrType).Invoke(null, [context])!);
                continue;
            }

            // A configuration that already reads CompanyId owns its own company scoping (EmployeeBase's
            // IsVisibleAcrossCompanies OR). Overwriting or ANDing here would break it.
            if (tenantWide || ReferencesCompanyId(existing))
                continue;

            // The 17 Accounting configurations replace the global filter outright; without this AND,
            // Bills, Vendors, Journals, Fixed Assets and MoMo keep no company scoping at all.
            var clause = (LambdaExpression)CompanyClauseBuilder
                .MakeGenericMethod(entityType.ClrType)
                .Invoke(null, [context])!;

            var parameter = existing.Parameters[0];
            var clauseBody = new ParameterRebinder(clause.Parameters[0], parameter).Visit(clause.Body)!;

            SetFilter(entityType, Expression.Lambda(Expression.AndAlso(existing.Body, clauseBody), parameter));
        }
    }

    // Conventions cannot overwrite an Explicit HasQueryFilter; the mutable metadata API can.
    private static void SetFilter(IConventionEntityType entityType, LambdaExpression filter)
    {
        ((IMutableEntityType)entityType).SetQueryFilter(filter);
    }

    private static bool ReferencesCompanyId(LambdaExpression filter)
    {
        var detector = new CompanyIdDetector();
        detector.Visit(filter.Body);
        return detector.Found;
    }

    /// <remarks>
    /// <paramref name="ctx"/> is the DbContext instance. Member access rooted on the context is
    /// re-evaluated per query as an <c>@__ef_filter__</c> parameter; closing over a service instead
    /// makes EF funcletize the value into a baked-in SQL constant shared across tenants.
    /// </remarks>
    private static LambdaExpression BuildFullFilter<TEntity>(IScopedQueryFilterContext ctx)
        where TEntity : AuditableEntity
    {
        Expression<Func<TEntity, bool>> filter = e =>
            e.DataStatus != DataState.Deleted
            && (e.IsGlobal || e.TenantId == ctx.CurrentTenantId)
            && (ctx.CompanyFilterActive == false || ctx.AllowedCompanyIds.Contains(e.CompanyId));

        return filter;
    }

    private static LambdaExpression BuildTenantFilter<TEntity>(IScopedQueryFilterContext ctx)
        where TEntity : AuditableEntity
    {
        Expression<Func<TEntity, bool>> filter = e =>
            e.DataStatus != DataState.Deleted
            && (e.IsGlobal || e.TenantId == ctx.CurrentTenantId);

        return filter;
    }

    private static LambdaExpression BuildCompanyClause<TEntity>(IScopedQueryFilterContext ctx)
        where TEntity : AuditableEntity
    {
        Expression<Func<TEntity, bool>> clause = e =>
            ctx.CompanyFilterActive == false || ctx.AllowedCompanyIds.Contains(e.CompanyId);

        return clause;
    }

    private sealed class ParameterRebinder(ParameterExpression from, ParameterExpression to) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == from ? to : base.VisitParameter(node);
        }
    }

    private sealed class CompanyIdDetector : ExpressionVisitor
    {
        public bool Found { get; private set; }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Member.Name == nameof(AuditableEntity.CompanyId)
                && node.Member.DeclaringType is not null
                && typeof(AuditableEntity).IsAssignableFrom(node.Member.DeclaringType))
            {
                Found = true;
            }

            return base.VisitMember(node);
        }
    }
}
