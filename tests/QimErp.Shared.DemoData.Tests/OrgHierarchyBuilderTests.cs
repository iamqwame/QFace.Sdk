using FluentAssertions;
using QimErp.Shared.DemoData.Industry;
using QimErp.Shared.DemoData.Industry.Profiles;
using Xunit;

namespace QimErp.Shared.DemoData.Tests;

public class OrgHierarchyBuilderTests
{
    // Constants mirror the privates inside OrgHierarchyBuilder.
    // If those values change in OrgHierarchyBuilder, mirror them here.
    private const int MinFanOut = 3;
    private const int MaxFanOut = 8;
    private const int MaxDepth = 15;

    [Fact]
    public void Build_IsDeterministic_ForSameSeed()
    {
        var profile = new BankingIndustryProfile();

        var first = profile.BuildOrgHierarchy(CompanyTier.Corporate, 1000, 42);
        var second = profile.BuildOrgHierarchy(CompanyTier.Corporate, 1000, 42);

        first.Nodes.Select(n => n.Code).Should().Equal(second.Nodes.Select(n => n.Code));
        first.Nodes.Select(n => n.Name).Should().Equal(second.Nodes.Select(n => n.Name));
        first.Nodes.Select(n => n.ParentCode).Should().Equal(second.Nodes.Select(n => n.ParentCode));
    }

    [Fact]
    public void Build_DepthDoesNotExceedMaxDepth()
    {
        var profile = new BankingIndustryProfile();

        var spec = profile.BuildOrgHierarchy(CompanyTier.Corporate, 10_000, 42);

        spec.Nodes.Should().OnlyContain(n => n.Level <= MaxDepth);
    }

    [Fact]
    public void Build_TotalHeadcountIsApproximatelyTarget()
    {
        const int target = 1000;
        var profile = new BankingIndustryProfile();

        var spec = profile.BuildOrgHierarchy(CompanyTier.Corporate, target, 42);

        var parentCodes = new HashSet<string>(
            spec.Nodes.Where(n => n.ParentCode is not null).Select(n => n.ParentCode!),
            StringComparer.OrdinalIgnoreCase);

        var leafHeadcountSum = spec.Nodes
            .Where(n => !parentCodes.Contains(n.Code))
            .Sum(n => n.TargetHeadcount);

        var lowerBound = (int)(target * 0.95);
        var upperBound = (int)(target * 1.05);
        leafHeadcountSum.Should().BeInRange(lowerBound, upperBound);
    }

    [Fact]
    public void Build_RootHasLevel1()
    {
        var profile = new BankingIndustryProfile();

        var spec = profile.BuildOrgHierarchy(CompanyTier.Corporate, 1000, 42);

        var roots = spec.Nodes.Where(n => n.ParentCode is null).ToList();
        roots.Should().HaveCount(1);
        roots[0].Level.Should().Be(1);
    }

    [Fact]
    public void Build_AllChildrenHaveTheirParent()
    {
        var profile = new BankingIndustryProfile();

        var spec = profile.BuildOrgHierarchy(CompanyTier.Corporate, 1000, 42);

        var codes = new HashSet<string>(
            spec.Nodes.Select(n => n.Code), StringComparer.OrdinalIgnoreCase);

        foreach (var node in spec.Nodes.Where(n => n.ParentCode is not null))
        {
            codes.Should().Contain(node.ParentCode!,
                $"node '{node.Code}' references parent '{node.ParentCode}' which must exist");
        }
    }

    [Fact]
    public void Build_FanOutIsBoundedBetween3and8()
    {
        var profile = new BankingIndustryProfile();

        var spec = profile.BuildOrgHierarchy(CompanyTier.Corporate, 1000, 42);

        // Baseline (L1-L4) units come from the industry profile and may have any
        // number of direct children — the MinFanOut/MaxFanOut bounds only apply
        // to the builder's own subdivisions. The builder emits child codes shaped
        // as "{parent.Code}.{suffix}", so we look at any non-baseline node and
        // count its siblings that share its parent AND the same dotted prefix.
        // (See OrgHierarchyBuilder.SubdivideNode for the constants.)
        var builderChildrenByParent = spec.Nodes
            .Where(n => n.ParentCode is not null
                && n.Code.StartsWith($"{n.ParentCode}.", StringComparison.OrdinalIgnoreCase))
            .GroupBy(n => n.ParentCode!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

        builderChildrenByParent.Should().NotBeEmpty(
            "the Corporate / 1000 scenario should produce at least one builder-driven subdivision");

        foreach (var (parentCode, count) in builderChildrenByParent)
        {
            count.Should().BeInRange(MinFanOut, MaxFanOut,
                $"parent '{parentCode}' was subdivided by the builder and must respect MinFanOut/MaxFanOut");
        }
    }

    [Fact]
    public void Build_StartupTier_HasShallowDepth()
    {
        var profile = new BankingIndustryProfile();

        var spec = profile.BuildOrgHierarchy(CompanyTier.Startup, 50, 42);

        spec.Nodes.Should().OnlyContain(n => n.Level <= 5);
    }
}
