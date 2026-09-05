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
        subscribers.Should().Contain(s => s.ModuleKey == ModuleKeys.Leave);
        subscribers.Should().NotContain(s => s.ModuleKey == ModuleKeys.Recruitment);
        subscribers.Should().Contain(s => s.ActivitySuffix == "IAM");
        subscribers.Should().Contain(s => s.ActivitySuffix == "TenantBilling");
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
    public void FilterByInstalled_StockIssue_InventoryOnlyWhenSelected()
    {
        var withInventory = ModuleSyncRegistry.FilterByInstalled(
            SyncType.StockIssue,
            [ModuleKeys.AccountsReceivable, ModuleKeys.Inventory]);
        withInventory.Should().ContainSingle(s => s.ModuleKey == ModuleKeys.Inventory);
        withInventory[0].TaskQueue.Should().Be("qimerp-inventory-stock-issue-sync");
        withInventory[0].ActivitySuffix.Should().Be("Inventory");

        var withoutInventory = ModuleSyncRegistry.FilterByInstalled(
            SyncType.StockIssue,
            [ModuleKeys.AccountsReceivable, ModuleKeys.CoreAccounting]);
        withoutInventory.Should().BeEmpty();
    }

    [Fact]
    public void FilterByInstalled_Customer_InventoryOnlyWhenSelected()
    {
        var withInventory = ModuleSyncRegistry.FilterByInstalled(
            SyncType.Customer,
            [ModuleKeys.AccountsReceivable, ModuleKeys.Inventory]);
        withInventory.Should().ContainSingle(s => s.ModuleKey == ModuleKeys.Inventory);
        withInventory[0].TaskQueue.Should().Be("qimerp-inventory-customer-sync");
        withInventory[0].ActivitySuffix.Should().Be("Inventory");

        var withoutInventory = ModuleSyncRegistry.FilterByInstalled(
            SyncType.Customer,
            [ModuleKeys.AccountsReceivable, ModuleKeys.CoreAccounting]);
        withoutInventory.Should().BeEmpty();
    }

    [Fact]
    public void FilterByInstalled_Vendor_InventoryOnlyWhenSelected()
    {
        var withInventory = ModuleSyncRegistry.FilterByInstalled(
            SyncType.Vendor,
            [ModuleKeys.AccountsPayable, ModuleKeys.Inventory]);
        withInventory.Should().ContainSingle(s => s.ModuleKey == ModuleKeys.Inventory);
        withInventory[0].TaskQueue.Should().Be("qimerp-inventory-vendor-sync");
        withInventory[0].ActivitySuffix.Should().Be("Inventory");

        var withoutInventory = ModuleSyncRegistry.FilterByInstalled(
            SyncType.Vendor,
            [ModuleKeys.AccountsPayable, ModuleKeys.CoreAccounting]);
        withoutInventory.Should().BeEmpty();
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

    [Fact]
    public void TryResolveAdminBackfillStep_Benefit_ResolvesToBenefitAdminSync()
    {
        var subscription = ModuleSyncRegistry.TryResolveAdminBackfillStep("EnsureAdminDataInBenefit");

        subscription.Should().NotBeNull();
        subscription!.ModuleKey.Should().Be(ModuleKeys.Benefits);
        subscription.ActivitySuffix.Should().Be("Benefit");
    }

    [Fact]
    public void ResolveSetupStepQueue_SyncSubscriptionModules_RoutesToTenantBillingSetup()
    {
        ModuleSyncRegistry.ResolveSetupStepQueue("SyncSubscriptionModules")
            .Should().Be("qimerp-iam-tenant-setup");
    }

    [Fact]
    public void IsPlugin_AllNineCatalogPlugins_ReturnTrue()
    {
        foreach (var key in PluginKeys.All)
        {
            ModuleSyncRegistry.IsPlugin(key).Should().BeTrue($"expected plugin for {key}");
        }
    }

    [Fact]
    public void ResolveDisableStep_Workflows_ReturnsDisableWorkflowModule()
    {
        ModuleSyncRegistry.ResolveDisableStep("workflows")
            .Should().Be("DisableWorkflowModule");
        ModuleSyncRegistry.ResolveSetupStepQueue("DisableWorkflowModule")
            .Should().Be("qimerp-workflow-tenant-setup");
    }

    [Fact]
    public void ResolveDisableStep_ConferenceNotify_ReturnsDisableActivity()
    {
        ModuleSyncRegistry.ResolveDisableStep(PluginKeys.ConferenceNotify)
            .Should().Be("DisablePluginConferenceNotify");
    }

    [Fact]
    public void ResolveSetupStepQueue_EnablePluginWebhookNotify_RoutesToWorkflowSetup()
    {
        ModuleSyncRegistry.ResolveSetupStepQueue("EnablePluginWebhookNotify")
            .Should().Be("qimerp-workflow-tenant-setup");
    }

    [Fact]
    public void ResolveDisableStep_ChatNotify_ReturnsDisableActivity()
    {
        ModuleSyncRegistry.ResolveDisableStep(PluginKeys.ChatNotify)
            .Should().Be("DisablePluginChatNotify");
    }

    [Fact]
    public void ResolveDisableStep_WebhookNotify_ReturnsDisableActivity()
    {
        ModuleSyncRegistry.ResolveDisableStep(PluginKeys.WebhookNotify)
            .Should().Be("DisablePluginWebhookNotify");
    }

    [Fact]
    public void ResolveSetupStepQueue_EnablePluginChatNotify_RoutesToWorkflowSetup()
    {
        ModuleSyncRegistry.ResolveSetupStepQueue("EnablePluginChatNotify")
            .Should().Be("qimerp-workflow-tenant-setup");
    }

    [Fact]
    public void ResolveDisableStep_SsnitFiling_ReturnsDisableActivity()
    {
        ModuleSyncRegistry.ResolveDisableStep(PluginKeys.SsnitFiling)
            .Should().Be("DisablePluginSsnitFiling");
    }

    [Fact]
    public void ResolveSetupStepQueue_EnablePluginEsign_RoutesToCoreHrSetup()
    {
        ModuleSyncRegistry.ResolveSetupStepQueue("EnablePluginEsign")
            .Should().Be("qimerp-corehr-employee-tenant-setup");
    }

    [Fact]
    public void TryResolveItemKey_Attendance_ReturnsCatalogSlug()
    {
        ModuleSyncRegistry.TryResolveItemKey(ModuleKeys.Attendance).Should().Be("attendance");
    }

    [Fact]
    public void Reporting_IsCatalogued_But_Is_Not_A_Runtime_Subscriber()
    {
        ModuleSyncRegistry.TryResolveItemKey(ModuleKeys.Reporting).Should().Be("reporting");
        ModuleSyncRegistry.ResolveSetupSteps("reporting").Should().BeEmpty();
        ModuleSyncRegistry.ResolveEmployeeBackfillStep("reporting").Should().BeNull();
        ModuleSyncRegistry.ResolveAdminDataBackfillStep("reporting").Should().BeNull();

        ModuleSyncRegistry.GetRuntimeSubscribers(SyncType.Employee)
            .Should().NotContain(s => s.ModuleKey == ModuleKeys.Reporting);
        ModuleSyncRegistry.GetRuntimeSubscribers(SyncType.AdminData)
            .Should().NotContain(s => s.ModuleKey == ModuleKeys.Reporting);
    }
}
