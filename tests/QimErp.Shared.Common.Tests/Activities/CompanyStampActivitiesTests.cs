using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QimErp.Shared.Common.Activities;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.Workflows;
using Temporalio.Testing;
using Xunit;

namespace QimErp.Shared.Common.Tests.Activities;

public sealed class CompanyStampActivitiesTests : IDisposable
{
    private const string Tenant = "tenant-stamp-0001";
    private const string OtherTenant = "tenant-stamp-0002";
    private const string Company = "company-a";

    private sealed class CompanyScopedRow : GuidAuditableEntity
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class TenantWideRow : GuidAuditableEntity, ITenantWideEntity
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class StampDbContext(DbContextOptions<StampDbContext> options) : DbContext(options)
    {
        public DbSet<CompanyScopedRow> CompanyScopedRows { get; set; } = null!;
        public DbSet<TenantWideRow> TenantWideRows { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<CompanyScopedRow>().Ignore(e => e.CustomFields);
            modelBuilder.Entity<TenantWideRow>().Ignore(e => e.CustomFields);
        }
    }

    private readonly StampDbContext _db = new(
        new DbContextOptionsBuilder<StampDbContext>()
            .UseInMemoryDatabase($"company-stamp-{Guid.NewGuid():N}")
            .Options);

    [Fact]
    public async Task StampsTheTenantsCompanyScopedRows_AndLeavesEverythingElseAlone()
    {
        _db.CompanyScopedRows.Add(Row<CompanyScopedRow>(Tenant, string.Empty));
        _db.CompanyScopedRows.Add(Row<CompanyScopedRow>(Tenant, "company-b"));
        _db.CompanyScopedRows.Add(Row<CompanyScopedRow>(OtherTenant, string.Empty));
        _db.TenantWideRows.Add(Row<TenantWideRow>(Tenant, string.Empty));
        await _db.SaveChangesAsync();

        var result = await RunAsync(new CompanyStampRequest { TenantId = Tenant, CompanyId = Company });

        result.Success.Should().BeTrue();
        result.RowsStamped.Should().Be(1);

        var rows = await _db.CompanyScopedRows.AsNoTracking().ToListAsync();
        rows.Single(r => r.TenantId == Tenant && r.CompanyId == Company).Should().NotBeNull();
        rows.Single(r => r.TenantId == OtherTenant).CompanyId.Should().BeEmpty("another tenant's rows are not this tenant's to stamp");
        (await _db.TenantWideRows.AsNoTracking().SingleAsync()).CompanyId
            .Should().BeEmpty("ITenantWideEntity rows must never be company-stamped");
    }

    [Fact]
    public async Task BlankCompanyId_IsRefused()
    {
        var result = await RunAsync(new CompanyStampRequest { TenantId = Tenant, CompanyId = " " });

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrWhiteSpace();
    }

    private Task<CompanyStampResult> RunAsync(CompanyStampRequest request)
    {
        var activities = new CompanyStampActivities<StampDbContext>(
            _db, NullLogger<CompanyStampActivities<StampDbContext>>.Instance);

        return new ActivityEnvironment().RunAsync(() => activities.StampCompanyOnExistingRowsAsync(request));
    }

    private static T Row<T>(string tenantId, string companyId) where T : GuidAuditableEntity, new() =>
        new() { Id = Guid.NewGuid(), TenantId = tenantId, CompanyId = companyId };

    public void Dispose() => _db.Dispose();
}
