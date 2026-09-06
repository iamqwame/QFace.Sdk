using System.Linq.Expressions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.Services.MultiTenancy;
using Xunit;
using Xunit.Abstractions;

namespace QimErp.Shared.Common.Tests.Database;

public sealed class TenantCompanyQueryFilterConventionTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly ICompanyContext _companyContext = new CompanyContext();

    public TenantCompanyQueryFilterConventionTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public void Dispose() => _companyContext.Clear();

    private static ConventionTestDbContext CreateContext(string tenantId = "tenant-a")
    {
        var tenantContext = new TenantContext();
        tenantContext.SetTenant(tenantId);

        var options = new DbContextOptionsBuilder<ConventionTestDbContext>()
            .UseNpgsql("Host=localhost;Database=convention_tests;Username=none;Password=none")
            .Options;

        return new ConventionTestDbContext(options, tenantContext);
    }

    private static IEnumerable<IEntityType> GuardedEntityTypes(ConventionTestDbContext context)
    {
        return context.Model.GetEntityTypes()
            .Where(e => typeof(AuditableEntity).IsAssignableFrom(e.ClrType))
            .Where(e => !e.IsOwned() && e.BaseType is null && e.FindPrimaryKey() is not null);
    }

    private static bool ReferencesCompanyId(LambdaExpression filter)
    {
        var detector = new CompanyIdProbe();
        detector.Visit(filter.Body);
        return detector.Found;
    }

    [Fact]
    public void Guard_every_auditable_entity_has_a_query_filter()
    {
        using var context = CreateContext();

        var unfiltered = GuardedEntityTypes(context)
            .Where(e => e.GetQueryFilter() is null)
            .Select(e => e.ClrType.Name)
            .ToArray();

        unfiltered.Should().BeEmpty();
    }

    [Fact]
    public void Guard_every_query_filter_scopes_company_unless_tenant_wide()
    {
        using var context = CreateContext();

        var missingCompanyClause = GuardedEntityTypes(context)
            .Where(e => !typeof(ITenantWideEntity).IsAssignableFrom(e.ClrType))
            .Where(e => e.GetQueryFilter() is not { } filter || !ReferencesCompanyId(filter))
            .Select(e => e.ClrType.Name)
            .ToArray();

        missingCompanyClause.Should().BeEmpty();
    }

    [Fact]
    public void Entity_reachable_only_through_a_configuration_is_filtered()
    {
        using var context = CreateContext();

        var filter = context.Model.FindEntityType(typeof(ConfigurationOnlyThing))!.GetQueryFilter();

        filter.Should().NotBeNull();
        ReferencesCompanyId(filter!).Should().BeTrue();
    }

    [Fact]
    public void Existing_custom_filter_is_anded_not_replaced()
    {
        using var context = CreateContext();

        var body = context.Model.FindEntityType(typeof(CustomFilteredThing))!.GetQueryFilter()!.Body;
        _output.WriteLine(body.ToString());

        body.NodeType.Should().Be(ExpressionType.AndAlso);
        var conjunction = (BinaryExpression)body;

        var original = conjunction.Left.ToString();
        original.Should().Contain("e.DataStatus").And.Contain("Active", "the Accounting predicate must survive");
        original.Should().Contain("e.IsGlobal").And.Contain("CurrentTenantId");
        ReferencesCompanyId(Expression.Lambda(conjunction.Left)).Should().BeFalse();

        conjunction.Right.ToString().Should().Contain("CompanyFilterActive").And.Contain("AllowedCompanyIds");
        ReferencesCompanyId(Expression.Lambda(conjunction.Right)).Should().BeTrue();
    }

    [Fact]
    public void Existing_custom_filter_uses_a_single_parameter()
    {
        using var context = CreateContext();

        var filter = context.Model.FindEntityType(typeof(CustomFilteredThing))!.GetQueryFilter()!;

        filter.Parameters.Should().HaveCount(1);
        var collector = new ParameterProbe();
        collector.Visit(filter.Body);
        collector.Parameters.Should().BeEquivalentTo(new[] { filter.Parameters[0] });
    }

    [Fact]
    public void Filter_that_already_references_company_id_is_left_untouched()
    {
        using var context = CreateContext();

        var body = context.Model.FindEntityType(typeof(SelfScopedThing))!.GetQueryFilter()!.Body.ToString();
        _output.WriteLine(body);

        body.Should().Contain("IsVisibleAcrossCompanies");
        CountOccurrences(body, "AllowedCompanyIds").Should().Be(1, "no second company clause may be appended");
    }

    [Fact]
    public void Tenant_wide_entity_gets_no_company_clause()
    {
        using var context = CreateContext();

        var filter = context.Model.FindEntityType(typeof(TenantWideThing))!.GetQueryFilter()!;
        _output.WriteLine(filter.Body.ToString());

        filter.Body.ToString().Should().Contain("CurrentTenantId");
        ReferencesCompanyId(filter).Should().BeFalse();
    }

    [Fact]
    public void Owned_and_derived_types_are_skipped()
    {
        using var context = CreateContext();

        context.Model.FindEntityType(typeof(DerivedThing))!.BaseType.Should().NotBeNull();

        var owned = context.Model.GetEntityTypes().Where(e => e.IsOwned()).ToArray();
        owned.Should().NotBeEmpty();
        owned.Should().OnlyContain(e => e.GetQueryFilter() == null);
    }

    [Fact]
    public void Filter_sql_uses_one_array_parameter_and_a_shape_stable_plan()
    {
        using var context = CreateContext();

        _companyContext.SetScope(CompanyScope.ForCompanies(["company-a", "company-b"], "company-a"));
        var twoCompanies = context.PlainThings.OrderBy(e => e.Name).ToQueryString();

        _companyContext.SetScope(CompanyScope.ForCompanies(
            ["c1", "c2", "c3", "c4", "c5"], "c1"));
        var fiveCompanies = context.PlainThings.OrderBy(e => e.Name).ToQueryString();

        _companyContext.SetScope(CompanyScope.AllCompanies(null));
        var allCompanies = context.PlainThings.OrderBy(e => e.Name).ToQueryString();

        _output.WriteLine("--- two companies ---");
        _output.WriteLine(twoCompanies);

        twoCompanies.Should().Contain("= ANY (@__ef_filter__AllowedCompanyIds");
        twoCompanies.Should().Contain("@__ef_filter__CurrentTenantId");
        twoCompanies.Should().NotContain("IN (");

        SqlBody(fiveCompanies).Should().Be(SqlBody(twoCompanies), "company count must not change the plan");
        SqlBody(allCompanies).Should().Be(SqlBody(twoCompanies), "All Companies must not change the plan");
    }

    private static string SqlBody(string queryString)
    {
        var lines = queryString.Split('\n').Where(l => !l.TrimStart().StartsWith('-'));
        return string.Join('\n', lines).Trim();
    }

    private static int CountOccurrences(string haystack, string needle)
    {
        var count = 0;
        var index = haystack.IndexOf(needle, StringComparison.Ordinal);
        while (index >= 0)
        {
            count++;
            index = haystack.IndexOf(needle, index + needle.Length, StringComparison.Ordinal);
        }

        return count;
    }

    private sealed class CompanyIdProbe : ExpressionVisitor
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

    private sealed class ParameterProbe : ExpressionVisitor
    {
        public HashSet<ParameterExpression> Parameters { get; } = [];

        protected override Expression VisitParameter(ParameterExpression node)
        {
            Parameters.Add(node);
            return base.VisitParameter(node);
        }
    }
}
