using FluentAssertions;
using QimErp.Shared.Common.Sync;
using QimErp.Shared.Common.TenantSetup;
using Xunit;

namespace QimErp.Shared.Common.Tests.Sync;

public class ModuleSyncRegistryTests
{
    [Fact]
    public void FilterByInstalled_Employee_OnlyPayrollWhenPayrollSelected()
    {
        var modules = new List<string> { ModuleKeys.CoreHR, ModuleKeys.Payroll };

        var subscribers = ModuleSyncRegistry.FilterByInstalled(SyncType.Employee, modules);

        subscribers.Should().Contain(s => s.ModuleKey == ModuleKeys.Payroll);
        subscribers.Should().NotContain(s => s.ModuleKey == ModuleKeys.Leave);
        subscribers.Should().Contain(s => s.ActivitySuffix == "IAM");
        subscribers.Should().Contain(s => s.ActivitySuffix == "TenantBilling");
    }

    [Fact]
    public void FilterByInstalled_GlReference_RequiresModuleInstall()
    {
        var modules = new List<string> { ModuleKeys.CoreAccounting, ModuleKeys.Project };

        var subscribers = ModuleSyncRegistry.FilterByInstalled(SyncType.GlReference, modules);

        subscribers.Should().ContainSingle(s => s.ModuleKey == ModuleKeys.Project);
        subscribers.Should().NotContain(s => s.ModuleKey == ModuleKeys.BudgetPlanning);
    }

    [Fact]
    public void FilterByInstalled_AssignmentChanged_PayrollOnlyWhenSelected()
    {
        var withPayroll = ModuleSyncRegistry.FilterByInstalled(
            SyncType.AssignmentChanged,
            [ModuleKeys.CoreHR, ModuleKeys.Payroll]);
        withPayroll.Should().ContainSingle(s => s.ModuleKey == ModuleKeys.Payroll);

        var withoutPayroll = ModuleSyncRegistry.FilterByInstalled(
            SyncType.AssignmentChanged,
            [ModuleKeys.CoreHR, ModuleKeys.Leave]);
        withoutPayroll.Should().BeEmpty();
    }

    [Fact]
    public void TryResolveItemKey_MapsModuleKeysToCatalogSlugs()
    {
        ModuleSyncRegistry.TryResolveItemKey(ModuleKeys.Payroll).Should().Be("payroll");
        ModuleSyncRegistry.TryResolveItemKey(ModuleKeys.BudgetPlanning).Should().Be("budget-planning");
    }

    [Fact]
    public void ResolveAdminDataBackfillStep_ReturnsStepForPayroll()
    {
        ModuleSyncRegistry.ResolveAdminDataBackfillStep("payroll")
            .Should().Be("EnsureAdminDataInPayroll");
    }
}
