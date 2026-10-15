using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.Extensions;
using QimErp.Shared.Common.Services.MultiTenancy;
using Xunit;

namespace QimErp.Shared.Common.Tests.Database;

public sealed class EnumConventionTests
{
    private static ConventionTestDbContext CreateContext()
    {
        var tenantContext = new TenantContext();
        tenantContext.SetTenant("tenant-enum");

        var options = new DbContextOptionsBuilder<ConventionTestDbContext>()
            .UseQimErpNpgsql("Host=localhost;Database=enum_convention_tests;Username=none;Password=none")
            .Options;

        return new ConventionTestDbContext(options, tenantContext);
    }

    private static Type? ProviderClrType(Microsoft.EntityFrameworkCore.Metadata.IProperty property)
        => (property.GetValueConverter() ?? property.GetTypeMapping().Converter)?.ProviderClrType;

    [Fact]
    public void Unconventioned_enum_property_stores_as_string()
    {
        using var context = CreateContext();

        var status = context.Model.FindEntityType(typeof(EnumConventionThing))!
            .FindProperty(nameof(EnumConventionThing.Status))!;

        ProviderClrType(status).Should().Be(typeof(string));
        status.GetColumnType().Should().BeOneOf("text", "character varying");
    }

    [Fact]
    public void Import_Status_remains_int_via_explicit_override()
    {
        using var context = CreateContext();

        var status = context.Model.FindEntityType(typeof(Import))!
            .FindProperty(nameof(Import.Status))!;

        ProviderClrType(status).Should().Be(typeof(int));
        status.GetColumnType().Should().Be("integer");
    }

    [Fact]
    public void DataState_on_auditable_entity_remains_string()
    {
        using var context = CreateContext();

        var dataStatus = context.Model.FindEntityType(typeof(EntityCodeConfig))!
            .FindProperty(nameof(AuditableEntity.DataStatus))!;

        ProviderClrType(dataStatus).Should().Be(typeof(string));
    }
}
