using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using QimErp.Shared.Common.Database;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.Interceptors;
using QimErp.Shared.Common.Services;
using QimErp.Shared.Common.Services.Auth;
using QimErp.Shared.Common.Services.Cache;
using QimErp.Shared.Common.Services.MultiTenancy;
using QimErp.Shared.Common.Tests.TenantSetup;
using QimErp.Shared.Common.Workflow.Entities;
using Xunit;

namespace QimErp.Shared.Common.Tests.Services;

public sealed class EntityCodeReservationTests : IDisposable
{
    private const string Tenant = "019e31ec-entcode-rsv-000000000001";

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options, ITenantContext tenantContext)
        : ApplicationDbContext<TestDbContext>(options, tenantContext)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppSetting>().Ignore(e => e.CustomFields);
            modelBuilder.Entity<EntityCodeConfig>().Ignore(e => e.CustomFields);
            modelBuilder.Entity<EntityCodeReservation>().Ignore(e => e.CustomFields);
            modelBuilder.Entity<TenantPluginFlag>().Ignore(e => e.CustomFields);
            modelBuilder.Entity<Import>().Ignore(e => e.CustomFields);
            modelBuilder.Entity<EntityWorkflowStep>().Ignore(e => e.CustomFields);
        }
    }

    private sealed class TestEntityCodeService(TestDbContext context, IDistributedCacheService cache)
        : EntityCodeService<TestDbContext>(
            context, cache, NullLogger.Instance, "TestModule",
            new Dictionary<string, (string, string, bool, int)>(StringComparer.OrdinalIgnoreCase)
            {
                ["Invoice"] = ("INV", "-", false, 4),
            });

    private sealed class Harness : IDisposable
    {
        private readonly ServiceProvider _root;
        private readonly IServiceScope _scope;

        public TestDbContext Db { get; }
        public TestEntityCodeService Service { get; }

        public Harness()
        {
            var services = new ServiceCollection();
            services.AddHttpContextAccessor();
            services.AddScoped<UserContextService>();
            services.AddScoped<ICurrentUserService>(sp => sp.GetRequiredService<UserContextService>());
            services.AddScoped<ITenantContext, TenantContext>();
            services.AddLogging();

            _root = services.BuildServiceProvider();
            _scope = _root.CreateScope();
            var sp = _scope.ServiceProvider;

            var userService = sp.GetRequiredService<UserContextService>();
            userService.SetContext(Tenant, "tester@qimerp.com");
            var tenantContext = sp.GetRequiredService<ITenantContext>();
            tenantContext.SetTenant(Tenant);

            var interceptor = new AuditEntitySaveChangesInterceptor(
                userService, NullLogger<AuditEntitySaveChangesInterceptor>.Instance, sp);

            Db = new TestDbContext(
                new DbContextOptionsBuilder<TestDbContext>()
                    .UseInMemoryDatabase($"entitycode-rsv-{Guid.NewGuid()}")
                    .AddInterceptors(interceptor)
                    .Options,
                tenantContext);

            Service = new TestEntityCodeService(Db, new InMemoryDistributedCacheService());
            new CompanyContext().SetScope(CompanyScope.Inactive);
        }

        public void Dispose()
        {
            Db.Dispose();
            _scope.Dispose();
            _root.Dispose();
        }
    }

    public void Dispose() => new CompanyContext().Clear();

    [Fact(DisplayName = "ValidateManualAsync accepts a round-tripped formatted code")]
    public async Task ValidateManual_AcceptsRoundTrippedCode()
    {
        using var harness = new Harness();
        await harness.Service.UpsertConfigAsync(
            Tenant, "Invoice", "INV", "-", false, 4,
            CodeGenerationMode.Manual, CodeResetPeriod.Never);

        var (isValid, error) = await harness.Service.ValidateManualAsync(Tenant, "Invoice", "INV-0007");
        isValid.Should().BeTrue();
        error.Should().BeNull();
    }

    [Fact(DisplayName = "ValidateManualAsync rejects empty and malformed codes")]
    public async Task ValidateManual_RejectsEmptyAndMalformed()
    {
        using var harness = new Harness();

        var empty = await harness.Service.ValidateManualAsync(Tenant, "Invoice", "  ");
        empty.IsValid.Should().BeFalse();

        var bad = await harness.Service.ValidateManualAsync(Tenant, "Invoice", "WRONG-0001");
        bad.IsValid.Should().BeFalse();
        bad.Error.Should().NotBeNullOrWhiteSpace();
    }

    [Fact(DisplayName = "TryReserveManualAsync blocks a second token from the same code")]
    public async Task Reserve_Collision_FailsForOtherToken()
    {
        using var harness = new Harness();
        await harness.Service.UpsertConfigAsync(
            Tenant, "Invoice", "INV", "-", false, 4,
            CodeGenerationMode.Manual, CodeResetPeriod.Never);

        var first = await harness.Service.TryReserveManualAsync(
            Tenant, "Invoice", "INV-0042", "token-a", EntityCodeReservation.DefaultTtl);
        first.Reserved.Should().BeTrue();

        var second = await harness.Service.TryReserveManualAsync(
            Tenant, "Invoice", "INV-0042", "token-b", EntityCodeReservation.DefaultTtl);
        second.Reserved.Should().BeFalse();
        second.Error.Should().Contain("reserved");

        var refresh = await harness.Service.TryReserveManualAsync(
            Tenant, "Invoice", "INV-0042", "token-a", EntityCodeReservation.DefaultTtl);
        refresh.Reserved.Should().BeTrue();
    }

    [Fact(DisplayName = "SuggestAsync skips a code reserved by another token")]
    public async Task Suggest_SkipsReservedCode()
    {
        using var harness = new Harness();
        await harness.Service.UpsertConfigAsync(
            Tenant, "Invoice", "INV", "-", false, 4,
            CodeGenerationMode.Auto, CodeResetPeriod.Never);

        // Seed LastSequence=0 so next suggest is INV-0001; reserve that without advancing HWM.
        var reserved = await harness.Service.TryReserveManualAsync(
            Tenant, "Invoice", "INV-0001", "other-token", EntityCodeReservation.DefaultTtl,
            validateFormat: false);
        reserved.Reserved.Should().BeTrue();

        var suggested = await harness.Service.SuggestAsync(Tenant, "Invoice");
        suggested.Should().Be("INV-0002");
    }

    [Fact(DisplayName = "ChartOfAccount reservations are tenant-wide even under a company scope")]
    public async Task ChartOfAccount_Reservations_AreTenantWide()
    {
        using var harness = new Harness();
        new CompanyContext().SetScope(CompanyScope.ForCompanies(["company-a"], "company-a"));

        var reserved = await harness.Service.TryReserveManualAsync(
            Tenant, "ChartOfAccount", "1000", "token-a", EntityCodeReservation.DefaultTtl,
            validateFormat: false);
        reserved.Reserved.Should().BeTrue();

        var row = await harness.Db.Set<EntityCodeReservation>()
            .IgnoreQueryFilters()
            .SingleAsync(r => r.TenantId == Tenant && r.Code == "1000" && r.DataStatus == DataState.Active);
        row.CompanyId.Should().BeEmpty();

        new CompanyContext().SetScope(CompanyScope.ForCompanies(["company-b"], "company-b"));
        var blocked = await harness.Service.IsCodeReservedByOtherAsync(
            Tenant, "ChartOfAccount", "1000", myToken: "token-b");
        blocked.Should().BeTrue();

        var second = await harness.Service.TryReserveManualAsync(
            Tenant, "ChartOfAccount", "1000", "token-b", EntityCodeReservation.DefaultTtl,
            validateFormat: false);
        second.Reserved.Should().BeFalse();
        second.Error.Should().Contain("reserved");
    }

    [Fact(DisplayName = "TryReserveManualAsync clamps TTL above MaxTtl")]
    public async Task Reserve_ClampsExcessiveTtl()
    {
        using var harness = new Harness();
        var before = DateTimeOffset.UtcNow;

        var reserved = await harness.Service.TryReserveManualAsync(
            Tenant, "ChartOfAccount", "2000", "token-ttl", TimeSpan.FromHours(2),
            validateFormat: false);
        reserved.Reserved.Should().BeTrue();

        var row = await harness.Db.Set<EntityCodeReservation>()
            .IgnoreQueryFilters()
            .SingleAsync(r => r.TenantId == Tenant && r.Code == "2000" && r.DataStatus == DataState.Active);

        row.ExpiresAt.Should().BeOnOrBefore(before.Add(EntityCodeReservation.MaxTtl).AddSeconds(5));
        row.ExpiresAt.Should().BeOnOrAfter(before.Add(EntityCodeReservation.MaxTtl).AddSeconds(-5));
    }

    [Fact(DisplayName = "TryReserveManualAsync rejects when active reservation cap is reached")]
    public async Task Reserve_RejectsWhenActiveCapReached()
    {
        using var harness = new Harness();

        for (var i = 0; i < EntityCodeReservation.MaxActiveReservationsPerScope; i++)
        {
            var result = await harness.Service.TryReserveManualAsync(
                Tenant, "ChartOfAccount", $"{3000 + i}", $"token-{i}",
                EntityCodeReservation.DefaultTtl, validateFormat: false);
            result.Reserved.Should().BeTrue($"reservation {i} should succeed");
        }

        var blocked = await harness.Service.TryReserveManualAsync(
            Tenant, "ChartOfAccount", "3999", "token-overflow",
            EntityCodeReservation.DefaultTtl, validateFormat: false);
        blocked.Reserved.Should().BeFalse();
        blocked.Error.Should().Contain("Too many active code reservations");

        var refresh = await harness.Service.TryReserveManualAsync(
            Tenant, "ChartOfAccount", "3000", "token-0",
            EntityCodeReservation.DefaultTtl, validateFormat: false);
        refresh.Reserved.Should().BeTrue();
    }
}
