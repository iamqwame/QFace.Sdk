using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using QimErp.Shared.Common.Database;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.ExceptionHandlers;
using QimErp.Shared.Common.Interceptors;
using QimErp.Shared.Common.Services;
using QimErp.Shared.Common.Services.Auth;
using QimErp.Shared.Common.Services.Cache;
using QimErp.Shared.Common.Services.MultiTenancy;
using QimErp.Shared.Common.Tests.TenantSetup;
using QimErp.Shared.Common.Workflow.Entities;
using Xunit;

namespace QimErp.Shared.Common.Tests.Services;

public sealed class EntityCodeServiceMultiCompanyTests : IDisposable
{
    private const string Tenant = "019e31ec-entcode-0000-000000000001";
    private const string CompanyA = "company-a";
    private const string CompanyB = "company-b";

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options, ITenantContext tenantContext)
        : ApplicationDbContext<TestDbContext>(options, tenantContext)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppSetting>().Ignore(e => e.CustomFields);
            modelBuilder.Entity<EntityCodeConfig>().Ignore(e => e.CustomFields);
            modelBuilder.Entity<TenantPluginFlag>().Ignore(e => e.CustomFields);
            modelBuilder.Entity<Import>().Ignore(e => e.CustomFields);
            modelBuilder.Entity<EntityWorkflowStep>().Ignore(e => e.CustomFields);
        }
    }

    private sealed class RecordingDistributedCacheService : IDistributedCacheService
    {
        private readonly InMemoryDistributedCacheService _inner = new();
        public List<string> TouchedKeys { get; } = [];

        public Task<T?> GetAsync<T>(string key)
        {
            TouchedKeys.Add(key);
            return _inner.GetAsync<T>(key);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            TouchedKeys.Add(key);
            return _inner.SetAsync(key, value, expiration);
        }

        public Task RemoveAsync(string key)
        {
            TouchedKeys.Add(key);
            return _inner.RemoveAsync(key);
        }

        public Task RemoveByPatternAsync(string pattern) => _inner.RemoveByPatternAsync(pattern);
        public Task<bool> ExistsAsync(string key) => _inner.ExistsAsync(key);
        public Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) =>
            throw new NotSupportedException();

        public Task<T?> GetAsync<T>(string key, string? region = null) => GetAsync<T>(key);
        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, string? region = null) =>
            SetAsync(key, value, expiration);
        public Task RemoveAsync(string key, string? region = null) => RemoveAsync(key);
        public Task RemoveByPatternAsync(string pattern, string? region = null) => RemoveByPatternAsync(pattern);
        public Task<bool> ExistsAsync(string key, string? region = null) => ExistsAsync(key);
        public Task<T> GetOrSetAsync<T>(
            string key, Func<Task<T>> factory, TimeSpan? expiration = null, string? region = null) =>
            throw new NotSupportedException();
    }

    private sealed class TestEntityCodeService(TestDbContext context, RecordingDistributedCacheService cache)
        : EntityCodeService<TestDbContext>(context, cache, NullLogger.Instance, "TestModule");

    private sealed class Harness : IDisposable
    {
        private readonly ServiceProvider _root;
        private readonly IServiceScope _scope;

        public TestDbContext Db { get; }
        public RecordingDistributedCacheService Cache { get; } = new();
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
                    .UseInMemoryDatabase($"entitycode-{Guid.NewGuid()}")
                    .AddInterceptors(interceptor)
                    .Options,
                tenantContext);

            Service = new TestEntityCodeService(Db, Cache);
        }

        public void Dispose()
        {
            Db.Dispose();
            _scope.Dispose();
            _root.Dispose();
        }
    }

    private static void SetScope(CompanyScope scope) => new CompanyContext().SetScope(scope);

    public void Dispose() => new CompanyContext().Clear();

    [Fact(DisplayName = "Two companies generate independent, sequential document codes")]
    public async Task TwoCompanies_GenerateIndependentSequences()
    {
        using var harness = new Harness();

        SetScope(CompanyScope.ForCompanies([CompanyA, CompanyB], CompanyA));
        var companyACode1 = await harness.Service.GenerateAsync(Tenant, "Invoice");
        var companyACode2 = await harness.Service.GenerateAsync(Tenant, "Invoice");

        SetScope(CompanyScope.ForCompanies([CompanyA, CompanyB], CompanyB));
        var companyBCode1 = await harness.Service.GenerateAsync(Tenant, "Invoice");

        companyACode1.Should().Be("INV-0001");
        companyACode2.Should().Be("INV-0002");
        companyBCode1.Should().Be("INV-0001");

        var configs = await harness.Db.EntityCodeConfigs.IgnoreQueryFilters()
            .Where(c => c.TenantId == Tenant && c.EntityType == "Invoice")
            .ToListAsync();

        configs.Should().HaveCount(2);
        configs.Should().Contain(c => c.CompanyId == CompanyA && c.LastSequence == 2);
        configs.Should().Contain(c => c.CompanyId == CompanyB && c.LastSequence == 1);
    }

    [Fact(DisplayName = "Company A's first code after multi-company enablement continues the tenant sequence, it does not restart")]
    public async Task FirstCompanyCode_ContinuesTenantSequence_DoesNotRestart()
    {
        using var harness = new Harness();

        SetScope(CompanyScope.Inactive);
        (await harness.Service.GenerateAsync(Tenant, "Invoice")).Should().Be("INV-0001");

        var tenantWide = await harness.Db.EntityCodeConfigs.IgnoreQueryFilters()
            .FirstAsync(c => c.EntityType == "Invoice" && c.CompanyId == string.Empty);
        tenantWide.IncrementSequenceBy(499);
        await harness.Db.SaveChangesAsync();

        SetScope(CompanyScope.ForCompanies([CompanyA, CompanyB], CompanyA));

        (await harness.Service.GenerateAsync(Tenant, "Invoice")).Should().Be("INV-0501");

        var companyConfig = await harness.Db.EntityCodeConfigs.IgnoreQueryFilters()
            .FirstAsync(c => c.EntityType == "Invoice" && c.CompanyId == CompanyA);
        companyConfig.LastSequence.Should().Be(501);
    }

    [Fact(DisplayName = "A two-company caller with no active company cannot auto-create a tenant-shared numbering config")]
    public async Task TwoCompanyCaller_NoActiveCompany_CannotWriteTenantSharedConfig()
    {
        using var harness = new Harness();

        SetScope(CompanyScope.ForCompanies([CompanyA, CompanyB], active: null));

        var act = async () => await harness.Service.GenerateAsync(Tenant, "Invoice");
        await act.Should().ThrowAsync<AppSettingScopeViolationException>();

        (await harness.Db.EntityCodeConfigs.IgnoreQueryFilters().ToListAsync()).Should().BeEmpty();
    }

    [Fact(DisplayName = "Cache keys for the same entity type differ between two companies in the same tenant")]
    public async Task CacheKeys_DifferBetweenCompanies()
    {
        using var harness = new Harness();

        SetScope(CompanyScope.ForCompanies([CompanyA, CompanyB], CompanyA));
        await harness.Service.GenerateAsync(Tenant, "Invoice");

        SetScope(CompanyScope.ForCompanies([CompanyA, CompanyB], CompanyB));
        await harness.Service.GenerateAsync(Tenant, "Invoice");

        var keys = harness.Cache.TouchedKeys.Where(k => k.Contains("Invoice")).Distinct().ToList();

        keys.Should().Contain(k => k.Contains($"company:{CompanyA}"));
        keys.Should().Contain(k => k.Contains($"company:{CompanyB}"));
    }
}
