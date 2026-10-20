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

/// <summary>
/// DATA-27: GenerateBatchAsync fast path + reservation skip behaviour.
/// </summary>
public sealed class EntityCodeGenerateBatchData27Tests : IDisposable
{
    private const string Tenant = "019e31ec-entcode-d27-000000000001";

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
                    .UseInMemoryDatabase($"entitycode-d27-{Guid.NewGuid()}")
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

    [Fact]
    public async Task GenerateBatch_returns_contiguous_codes_without_reservations()
    {
        using var harness = new Harness();
        await harness.Service.UpsertConfigAsync(
            Tenant, "Invoice", "INV", "-", false, 4,
            CodeGenerationMode.Auto, CodeResetPeriod.Never);

        var codes = await harness.Service.GenerateBatchAsync(Tenant, "Invoice", 5);

        codes.Should().Equal("INV-0001", "INV-0002", "INV-0003", "INV-0004", "INV-0005");

        var config = await harness.Db.Set<EntityCodeConfig>()
            .IgnoreQueryFilters()
            .SingleAsync(c => c.TenantId == Tenant && c.EntityType == "Invoice");
        config.LastSequence.Should().Be(5);
    }

    [Fact]
    public async Task GenerateBatch_skips_reserved_codes()
    {
        using var harness = new Harness();
        await harness.Service.UpsertConfigAsync(
            Tenant, "Invoice", "INV", "-", false, 4,
            CodeGenerationMode.Auto, CodeResetPeriod.Never);

        var reserved = await harness.Service.TryReserveManualAsync(
            Tenant, "Invoice", "INV-0001", "other-token", EntityCodeReservation.DefaultTtl,
            validateFormat: false);
        reserved.Reserved.Should().BeTrue();

        var codes = await harness.Service.GenerateBatchAsync(Tenant, "Invoice", 2);

        codes.Should().Equal("INV-0002", "INV-0003");
    }

    [Fact]
    public async Task GenerateBatch_count_one_uses_fast_path()
    {
        using var harness = new Harness();
        await harness.Service.UpsertConfigAsync(
            Tenant, "Invoice", "INV", "-", false, 4,
            CodeGenerationMode.Auto, CodeResetPeriod.Never);

        var codes = await harness.Service.GenerateBatchAsync(Tenant, "Invoice", 1);

        codes.Should().Equal("INV-0001");
    }
}
