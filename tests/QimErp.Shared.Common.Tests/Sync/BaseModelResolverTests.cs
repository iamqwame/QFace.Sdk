using FluentAssertions;
using QimErp.Shared.Common.Sync;
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

    [Fact]
    public void Resolve_PosExpandsToInventoryPrerequisite()
    {
        var resolved = BaseModelResolver.Resolve([ModuleKeys.POS]);

        resolved.Should().Contain(ModuleKeys.POS);
        resolved.Should().Contain(ModuleKeys.Inventory);
        resolved.Should().Contain(ModuleKeys.CoreHR);
        resolved.Should().Contain(ModuleKeys.Leave);
    }

    [Fact]
    public void ResolveFromCsv_PosOnlyTenantCsvExpandsToInventory()
    {
        var resolved = BaseModelResolver.ResolveFromCsv("CoreHR,Leave,POS");

        resolved.Should().Contain(ModuleKeys.Inventory);
    }

    [Theory]
    [InlineData(ModuleKeys.AccountsPayable)]
    [InlineData(ModuleKeys.AccountsReceivable)]
    [InlineData(ModuleKeys.BudgetPlanning)]
    [InlineData(ModuleKeys.CashManagement)]
    public void Resolve_AccountingSubLedgersExpandToCoreAccounting(string moduleKey)
    {
        var resolved = BaseModelResolver.Resolve([moduleKey]);

        resolved.Should().Contain(moduleKey);
        resolved.Should().Contain(ModuleKeys.CoreAccounting);
    }

    [Fact]
    public void Resolve_ReachesPrerequisiteClosureForEveryKnownModule()
    {
        foreach (var moduleKey in AllKnownModuleKeys)
        {
            var resolved = BaseModelResolver.Resolve([moduleKey]);

            foreach (var resolvedKey in resolved)
            {
                var itemKey = ModuleSyncRegistry.TryResolveItemKey(resolvedKey);
                if (itemKey is null)
                    continue;

                foreach (var prerequisite in ModuleSyncRegistry.ResolvePrerequisites(itemKey))
                {
                    resolved.Should().Contain(
                        prerequisite.ModuleKey,
                        "resolving '{0}' pulled in '{1}', whose prerequisite '{2}' must also be resolved",
                        moduleKey, resolvedKey, prerequisite.ModuleKey);
                }
            }

            BaseModelResolver.Resolve(resolved).Should().BeEquivalentTo(resolved);
        }

        BaseModelResolver.Resolve(AllKnownModuleKeys).Should().Contain(AllKnownModuleKeys);
    }

    [Fact]
    public void NormalizeForPersistence_DoesNotPersistDerivedPrerequisites()
    {
        var csv = BaseModelResolver.NormalizeForPersistence([ModuleKeys.POS]);

        csv.Should().Contain(ModuleKeys.POS);
        csv.Should().NotContain(ModuleKeys.Inventory);
        BaseModelResolver.ResolveFromCsv(csv).Should().Contain(ModuleKeys.Inventory);
    }

    [Fact]
    public void ResolveExplicit_PosOnlyTenant_DoesNotGrantInventory()
    {
        var resolved = BaseModelResolver.ResolveExplicit([ModuleKeys.CoreHR, ModuleKeys.Leave, ModuleKeys.POS]);

        resolved.Should().Contain(ModuleKeys.POS);
        resolved.Should().NotContain(ModuleKeys.Inventory);
    }

    [Theory]
    [InlineData(ModuleKeys.AccountsPayable)]
    [InlineData(ModuleKeys.AccountsReceivable)]
    [InlineData(ModuleKeys.BudgetPlanning)]
    [InlineData(ModuleKeys.CashManagement)]
    public void ResolveExplicit_AccountingSubLedgers_DoNotGrantCoreAccounting(string moduleKey)
    {
        var resolved = BaseModelResolver.ResolveExplicit([moduleKey]);

        resolved.Should().Contain(moduleKey);
        resolved.Should().NotContain(ModuleKeys.CoreAccounting);
    }

    [Fact]
    public void ResolveExplicitFromCsv_StripsUnknownTokensAndUnionsBaseWithoutExpanding()
    {
        var resolved = BaseModelResolver.ResolveExplicitFromCsv("BaseModules,POS");

        resolved.Should().BeEquivalentTo([ModuleKeys.CoreHR, ModuleKeys.Leave, ModuleKeys.POS]);
    }

    [Fact]
    public void ModuleGuard_IsExplicitlySelected_TreatsPrerequisitesAsUnentitled()
    {
        string[] posOnly = [ModuleKeys.CoreHR, ModuleKeys.Leave, ModuleKeys.POS];

        ModuleGuard.IsExplicitlySelected(posOnly, ModuleKeys.POS).Should().BeTrue();
        ModuleGuard.IsExplicitlySelected(posOnly, ModuleKeys.Leave).Should().BeTrue();
        ModuleGuard.IsExplicitlySelected(posOnly, ModuleKeys.Inventory).Should().BeFalse();
        ModuleGuard.IsExplicitlySelected(null, ModuleKeys.Inventory).Should().BeFalse();
    }

    [Fact]
    public void Resolve_NeverYieldsAModuleKeyOutsideTheAllowList()
    {
        foreach (var definition in ModuleSyncRegistry.GetAllModules())
        {
            BaseModelResolver.Resolve([definition.ModuleKey])
                .Should()
                .BeSubsetOf(
                    AllKnownModuleKeys,
                    "resolving '{0}' must not leak a module key the allow-list rejects",
                    definition.ModuleKey);
        }
    }

    private static readonly string[] AllKnownModuleKeys =
    [
        ModuleKeys.CoreHR,
        ModuleKeys.Payroll,
        ModuleKeys.Leave,
        ModuleKeys.Recruitment,
        ModuleKeys.Benefits,
        ModuleKeys.Surveys,
        ModuleKeys.EmployeeEngagement,
        ModuleKeys.Learning,
        ModuleKeys.Performance,
        ModuleKeys.Talent,
        ModuleKeys.WorkforcePlanning,
        ModuleKeys.Workflow,
        ModuleKeys.CoreAccounting,
        ModuleKeys.AccountsPayable,
        ModuleKeys.AccountsReceivable,
        ModuleKeys.BudgetPlanning,
        ModuleKeys.CashManagement,
        ModuleKeys.Inventory,
        ModuleKeys.Project,
        ModuleKeys.POS,
    ];
}
