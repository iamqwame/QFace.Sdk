using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.Services.MultiTenancy;
using Xunit;

namespace QimErp.Shared.Common.Tests.Database;

public sealed class EmployeeBaseConfigurationTests
{
    private static ConventionTestDbContext CreateContext()
    {
        var tenantContext = new TenantContext();
        tenantContext.SetTenant("tenant-a");

        var options = new DbContextOptionsBuilder<ConventionTestDbContext>()
            .UseNpgsql("Host=localhost;Database=convention_tests;Username=none;Password=none")
            .Options;

        return new ConventionTestDbContext(options, tenantContext);
    }

    [Fact]
    public void Code_uniqueness_is_scoped_per_company()
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(typeof(TestEmployee))!;
        var indexes = entityType.GetIndexes().ToArray();

        var codeUniqueIndexes = indexes
            .Where(i => i.IsUnique)
            .Where(i => i.Properties.Select(p => p.Name).Contains(nameof(EmployeeBase.Code)))
            .ToArray();

        codeUniqueIndexes.Should().ContainSingle();
        codeUniqueIndexes[0].Properties.Select(p => p.Name).Should().Equal(
            nameof(AuditableEntity.TenantId),
            nameof(AuditableEntity.CompanyId),
            nameof(EmployeeBase.Code));

        indexes.Should().NotContain(i =>
            i.IsUnique
            && i.Properties.Select(p => p.Name).SequenceEqual(new[] { nameof(AuditableEntity.TenantId), nameof(EmployeeBase.Code) }));
    }

    [Fact]
    public void Email_uniqueness_stays_tenant_wide()
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(typeof(TestEmployee))!;
        var indexes = entityType.GetIndexes().ToArray();

        indexes.Should().ContainSingle(i =>
            i.IsUnique
            && i.Properties.Select(p => p.Name).SequenceEqual(new[] { nameof(AuditableEntity.TenantId), nameof(EmployeeBase.Email) }));
    }

    [Fact]
    public void Visibility_index_exists_and_is_not_unique()
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(typeof(TestEmployee))!;
        var indexes = entityType.GetIndexes().ToArray();

        var visibilityIndex = indexes.SingleOrDefault(i =>
            i.Properties.Select(p => p.Name).SequenceEqual([
                nameof(AuditableEntity.TenantId),
                nameof(AuditableEntity.CompanyId),
                nameof(EmployeeBase.IsVisibleAcrossCompanies)
            ]));

        visibilityIndex.Should().NotBeNull();
        visibilityIndex!.IsUnique.Should().BeFalse();
    }
}
