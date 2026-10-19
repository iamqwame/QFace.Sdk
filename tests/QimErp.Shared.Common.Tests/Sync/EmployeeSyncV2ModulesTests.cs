using FluentAssertions;
using QimErp.Shared.Common.Sync;
using Xunit;

namespace QimErp.Shared.Common.Tests.Sync;

public sealed class EmployeeSyncV2ModulesTests
{
    [Theory]
    [InlineData("Learning")]
    [InlineData("Performance")]
    [InlineData("Talent")]
    [InlineData("WorkforcePlanning")]
    [InlineData("Payroll")]
    [InlineData("TenantBilling")]
    [InlineData("Leave")]
    [InlineData("Benefit")]
    [InlineData("Surveys")]
    [InlineData("Recruitment")]
    [InlineData("learning")]
    public void UsesV2_returns_true_for_migrated_modules(string suffix)
    {
        EmployeeSyncV2Modules.UsesV2(suffix).Should().BeTrue();
    }

    [Theory]
    [InlineData("IAM")]
    [InlineData("")]
    [InlineData("   ")]
    public void UsesV2_returns_false_for_unmigrated_or_empty(string suffix)
    {
        EmployeeSyncV2Modules.UsesV2(suffix).Should().BeFalse();
    }
}
