using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using QimErp.Shared.Common.AppSettings.Contracts;
using QimErp.Shared.Common.AppSettings.Features;
using QimErp.Shared.Common.AppSettings.Options;
using QimErp.Shared.Common.Database;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.Interceptors;
using QimErp.Shared.Common.Services;
using QimErp.Shared.Common.Services.Auth;
using QimErp.Shared.Common.Services.MultiTenancy;
using QimErp.Shared.Common.Workflow.Entities;
using Xunit;

namespace QimErp.Shared.Common.Tests.Services;

public sealed class CreateAppSettingHandlerTests : IDisposable
{
    private const string Tenant = "019e31ec-createset-0000-000000000001";
    private const string CompanyA = "company-a";

    private sealed record StubResponse;

    private sealed class StubMapper : IStructuredSettingsMapper<StubResponse>
    {
        public bool IsStructuredSettingKey(string key) => false;
        public string CategoryForKey(string key) => "General";
        public Dictionary<string, object> ToSettingsDictionary(StubResponse response) => [];
        public StubResponse ToStructuredResponse(Dictionary<string, string> values) => new();
    }

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

    private sealed class TestAppSettingsService(TestDbContext context, IMemoryCache cache)
        : AppSettingsService<TestDbContext>(context, NullLogger<AppSettingsService<TestDbContext>>.Instance, cache)
    {
        protected override DbSet<AppSetting> AppSettings => _context.AppSettings;
    }

    private sealed class Harness : IDisposable
    {
        private readonly ServiceProvider _root;
        private readonly IServiceScope _scope;

        public TestDbContext Db { get; }
        public TestAppSettingsService Service { get; }
        public CreateAppSettingHandler<StubResponse> Handler { get; }

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
                    .UseInMemoryDatabase($"createsetting-{Guid.NewGuid()}")
                    .AddInterceptors(interceptor)
                    .Options,
                tenantContext);

            Service = new TestAppSettingsService(Db, new MemoryCache(new MemoryCacheOptions()));

            Handler = new CreateAppSettingHandler<StubResponse>(
                NullLogger<CreateAppSettingHandler<StubResponse>>.Instance,
                Service,
                new StubMapper(),
                new StructuredAppSettingsApiOptions<StubResponse>(),
                new CreateAppSettingCommandValidator());
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

    [Fact(DisplayName = "A company can create its own override for a key that exists only as a tenant default")]
    public async Task Create_CompanyOverride_ForKeyThatOnlyExistsAsTenantDefault()
    {
        using var harness = new Harness();

        SetScope(CompanyScope.Inactive);
        await harness.Service.SetStringSettingAsync("currency.code", "GHS", "General", "desc");

        SetScope(CompanyScope.ForCompanies([CompanyA], CompanyA));

        var result = await harness.Handler.Handle(
            new CreateAppSettingCommand { SettingKey = "currency.code", SettingValue = "USD" },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.SettingValue.Should().Be("USD");

        var rows = await harness.Db.AppSettings.IgnoreQueryFilters()
            .Where(s => s.Key == "currency.code")
            .ToListAsync();
        rows.Should().HaveCount(2);
        rows.Should().Contain(s => s.CompanyId == CompanyA && s.Value == "USD");
        rows.Should().Contain(s => s.CompanyId == string.Empty && s.Value == "GHS");
    }

    [Fact(DisplayName = "Creating a key the same company already owns is still rejected as AlreadyExists")]
    public async Task Create_SameCompanyDuplicate_IsRejected()
    {
        using var harness = new Harness();

        SetScope(CompanyScope.ForCompanies([CompanyA], CompanyA));
        await harness.Service.SetStringSettingAsync("currency.code", "USD", "General", "desc");

        var result = await harness.Handler.Handle(
            new CreateAppSettingCommand { SettingKey = "currency.code", SettingValue = "EUR" },
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}
