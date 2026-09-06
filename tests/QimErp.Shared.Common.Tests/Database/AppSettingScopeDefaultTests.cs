using QimErp.Shared.Common.Extensions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.Services.MultiTenancy;
using Xunit;

namespace QimErp.Shared.Common.Tests.Database;

public sealed class AppSettingScopeDefaultTests
{
    private static ConventionTestDbContext CreateContext()
    {
        var tenantContext = new TenantContext();
        tenantContext.SetTenant("tenant-a");

        var options = new DbContextOptionsBuilder<ConventionTestDbContext>()
            .UseQimErpNpgsql("Host=localhost;Database=app_setting_scope_tests;Username=none;Password=none")
            .Options;

        return new ConventionTestDbContext(options, tenantContext);
    }

    [Fact(DisplayName = "The Scope column carries an explicit CompanyOverridable default, so existing rows do not upgrade to TenantOnly")]
    public void ScopeColumn_HasExplicitCompanyOverridableDefault()
    {
        using var context = CreateContext();
        var property = context.Model.FindEntityType(typeof(AppSetting))!
            .FindProperty(nameof(AppSetting.Scope))!;

        var column = property.GetTableColumnMappings().First().Column;

        // The stored default is the converted provider value the migration scaffolds into the column.
        column.DefaultValue.Should().Be(nameof(AppSettingScope.CompanyOverridable));
    }

    [Fact(DisplayName = "A Scope column that upgraded as an empty string materialises as CompanyOverridable")]
    public void EmptyScopeColumn_MaterialisesAsCompanyOverridable()
    {
        var converter = new EnumToStringConverter<AppSettingScope>();

        converter.ConvertFromProvider(string.Empty).Should().Be(AppSettingScope.CompanyOverridable);
    }
}
