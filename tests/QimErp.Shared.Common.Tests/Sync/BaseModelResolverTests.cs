using FluentAssertions;
using QimErp.Shared.Common.TenantSetup;
using Xunit;

namespace QimErp.Shared.Common.Tests.Sync;

public class BaseModelResolverTests
{
    [Fact]
    public void Resolve_AlwaysIncludesBaseModelKeys()
    {
        var resolved = BaseModelResolver.Resolve([]);

        resolved.Should().Contain(ModuleKeys.CoreHR);
        resolved.Should().Contain(ModuleKeys.Leave);
    }

    [Fact]
    public void Resolve_StripsUnknownBaseModulesToken()
    {
        var resolved = BaseModelResolver.Resolve(["BaseModules"]);

        resolved.Should().Contain(ModuleKeys.CoreHR);
        resolved.Should().Contain(ModuleKeys.Leave);
        resolved.Should().NotContain("BaseModules");
        resolved.Should().HaveCount(2);
    }

    [Fact]
    public void Resolve_MergesExplicitAddonsWithBase()
    {
        var resolved = BaseModelResolver.Resolve([ModuleKeys.Payroll]);

        resolved.Should().Contain(ModuleKeys.CoreHR);
        resolved.Should().Contain(ModuleKeys.Leave);
        resolved.Should().Contain(ModuleKeys.Payroll);
    }

    [Fact]
    public void NormalizeForPersistence_StripsUnknownTokens()
    {
        var csv = BaseModelResolver.NormalizeForPersistence(["BaseModules", ModuleKeys.Payroll]);

        csv.Should().NotContain("BaseModules");
        csv.Should().Contain(ModuleKeys.CoreHR);
        csv.Should().Contain(ModuleKeys.Leave);
        csv.Should().Contain(ModuleKeys.Payroll);
    }

    [Fact]
    public void ModuleGuard_LeaveSelectedForBaseModelTenant()
    {
        ModuleGuard.IsSelected([ModuleKeys.CoreHR, ModuleKeys.Leave], ModuleKeys.Leave).Should().BeTrue();
        ModuleGuard.IsSelected([ModuleKeys.CoreHR, ModuleKeys.Leave], ModuleKeys.Payroll).Should().BeFalse();
    }

    [Fact]
    public void ModuleGuard_EmptyMeansBaseModelOnly()
    {
        ModuleGuard.IsSelected([], ModuleKeys.Leave).Should().BeTrue();
        ModuleGuard.IsSelected([], ModuleKeys.CoreHR).Should().BeTrue();
        ModuleGuard.IsSelected([], ModuleKeys.Payroll).Should().BeFalse();
        ModuleGuard.IsSelected(null, ModuleKeys.Recruitment).Should().BeFalse();
    }

    [Fact]
    public void ResolveFromCsv_StripsUnknownTokensAndUnionsBase()
    {
        var resolved = BaseModelResolver.ResolveFromCsv("BaseModules,Payroll");

        resolved.Should().Contain(ModuleKeys.CoreHR);
        resolved.Should().Contain(ModuleKeys.Leave);
        resolved.Should().Contain(ModuleKeys.Payroll);
        resolved.Should().NotContain("BaseModules");
    }
}
