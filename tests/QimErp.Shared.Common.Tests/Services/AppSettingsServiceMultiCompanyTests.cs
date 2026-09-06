using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using QimErp.Shared.Common.Database;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.ExceptionHandlers;
using QimErp.Shared.Common.Interceptors;
using QimErp.Shared.Common.Services;
using QimErp.Shared.Common.Services.Auth;
using QimErp.Shared.Common.Services.MultiTenancy;
using QimErp.Shared.Common.Workflow.Entities;
using Xunit;

namespace QimErp.Shared.Common.Tests.Services;

public sealed class AppSettingsServiceMultiCompanyTests : IDisposable
{
    private const string Tenant = "019e31ec-appset-0000-000000000001";
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
        public MemoryCache Cache { get; } = new(new MemoryCacheOptions());
        public TestAppSettingsService Service { get; }

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
                    .UseInMemoryDatabase($"appsettings-{Guid.NewGuid()}")
                    .AddInterceptors(interceptor)
                    .Options,
                tenantContext);

            Service = new TestAppSettingsService(Db, Cache);
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

    [Fact(DisplayName = "Two companies in the same tenant hold independent values for the same key")]
    public async Task TwoCompanies_HoldIndependentValues()
    {
        using var harness = new Harness();

        SetScope(CompanyScope.ForCompanies([CompanyA, CompanyB], CompanyA));
        await harness.Service.SetStringSettingAsync("currency.code", "GHS", "General", "desc");

        SetScope(CompanyScope.ForCompanies([CompanyA, CompanyB], CompanyB));
        await harness.Service.SetStringSettingAsync("currency.code", "USD", "General", "desc");

        SetScope(CompanyScope.ForCompanies([CompanyA, CompanyB], CompanyA));
        (await harness.Service.GetStringSettingAsync("currency.code")).Should().Be("GHS");

        SetScope(CompanyScope.ForCompanies([CompanyA, CompanyB], CompanyB));
        (await harness.Service.GetStringSettingAsync("currency.code")).Should().Be("USD");

        var allRows = await harness.Db.AppSettings.IgnoreQueryFilters()
            .Where(s => s.Key == "currency.code")
            .ToListAsync();
        allRows.Should().HaveCount(2);
    }

    [Fact(DisplayName = "A company override wins locally and leaves the tenant default row untouched")]
    public async Task CompanyOverride_WinsOverTenantDefault_TenantDefaultUntouched()
    {
        using var harness = new Harness();

        SetScope(CompanyScope.Inactive);
        await harness.Service.SetStringSettingAsync("payroll.cycle", "Monthly", "Payroll", "desc");

        SetScope(CompanyScope.ForCompanies([CompanyA], CompanyA));
        await harness.Service.SetStringSettingAsync("payroll.cycle", "Weekly", "Payroll", "desc");

        (await harness.Service.GetStringSettingAsync("payroll.cycle")).Should().Be("Weekly");

        SetScope(CompanyScope.Inactive);
        (await harness.Service.GetStringSettingAsync("payroll.cycle")).Should().Be("Monthly");
    }

    [Fact(DisplayName = "All-companies scope with no active company resolves the tenant default only")]
    public async Task AllCompaniesScope_NoActiveCompany_ResolvesTenantDefaultOnly()
    {
        using var harness = new Harness();

        SetScope(CompanyScope.Inactive);
        await harness.Service.SetStringSettingAsync("leave.approvalLevels", "1", "Leave", "desc");

        SetScope(CompanyScope.ForCompanies([CompanyA], CompanyA));
        await harness.Service.SetStringSettingAsync("leave.approvalLevels", "2", "Leave", "desc");

        SetScope(CompanyScope.ForCompanies([CompanyA, CompanyB], active: null));

        (await harness.Service.GetStringSettingAsync("leave.approvalLevels")).Should().Be("1");
    }

    [Fact(DisplayName = "A TenantOnly key rejects a company override with zero rows written, but the tenant-default write still succeeds")]
    public async Task TenantOnlySetting_CompanyOverrideThrows_TenantDefaultWriteStillSucceeds()
    {
        using var harness = new Harness();

        SetScope(CompanyScope.Inactive);
        await harness.Service.SetStringSettingAsync("security.mfaRequired", "true", "Security", "desc");

        var tenantDefault = await harness.Db.AppSettings.IgnoreQueryFilters()
            .FirstAsync(s => s.Key == "security.mfaRequired" && s.CompanyId == string.Empty);
        tenantDefault.WithScope(AppSettingScope.TenantOnly);
        await harness.Db.SaveChangesAsync();

        SetScope(CompanyScope.ForCompanies([CompanyA], CompanyA));

        var act = async () => await harness.Service.SetStringSettingAsync(
            "security.mfaRequired", "false", "Security", "desc");
        await act.Should().ThrowAsync<AppSettingScopeViolationException>();

        var allRows = await harness.Db.AppSettings.IgnoreQueryFilters()
            .Where(s => s.Key == "security.mfaRequired")
            .ToListAsync();
        allRows.Should().ContainSingle();

        SetScope(CompanyScope.Inactive);
        await harness.Service.SetStringSettingAsync("security.mfaRequired", "false", "Security", "desc");
        (await harness.Service.GetStringSettingAsync("security.mfaRequired")).Should().Be("false");
    }

    [Fact(DisplayName = "A company delete with no override of its own leaves the tenant-shared row, and company B's value, intact")]
    public async Task CompanyDelete_LeavesTenantSharedRow_AndCompanyBValue_Intact()
    {
        using var harness = new Harness();

        SetScope(CompanyScope.Inactive);
        await harness.Service.SetStringSettingAsync("payroll.cycle", "Monthly", "Payroll", "desc");

        SetScope(CompanyScope.ForCompanies([CompanyA, CompanyB], CompanyA));
        await harness.Service.DeleteSettingAsync("payroll.cycle");

        SetScope(CompanyScope.ForCompanies([CompanyA, CompanyB], CompanyB));
        (await harness.Service.GetStringSettingAsync("payroll.cycle")).Should().Be("Monthly");

        var shared = await harness.Db.AppSettings.IgnoreQueryFilters()
            .FirstAsync(s => s.Key == "payroll.cycle" && s.CompanyId == string.Empty);
        shared.DataStatus.Should().Be(DataState.Active);
    }

    [Fact(DisplayName = "A TenantOnly key ignores a company row that already existed when the key was marked")]
    public async Task TenantOnlySetting_PreExistingCompanyRow_NeverWinsOnRead()
    {
        using var harness = new Harness();

        SetScope(CompanyScope.ForCompanies([CompanyA], CompanyA));
        await harness.Service.SetStringSettingAsync("security.mfaRequired", "false", "Security", "desc");

        SetScope(CompanyScope.Inactive);
        await harness.Service.SetStringSettingAsync("security.mfaRequired", "true", "Security", "desc");

        var shared = await harness.Db.AppSettings.IgnoreQueryFilters()
            .FirstAsync(s => s.Key == "security.mfaRequired" && s.CompanyId == string.Empty);
        shared.WithScope(AppSettingScope.TenantOnly);
        await harness.Db.SaveChangesAsync();

        SetScope(CompanyScope.ForCompanies([CompanyA], CompanyA));

        (await harness.Service.GetStringSettingAsync("security.mfaRequired")).Should().Be("true");
        (await harness.Service.GetSettingEntityAsync("security.mfaRequired"))!.CompanyId.Should().BeEmpty();
    }

    [Fact(DisplayName = "A two-company caller with no active company cannot write a tenant-shared setting")]
    public async Task TwoCompanyCaller_NoActiveCompany_CannotWriteTenantSharedSetting()
    {
        using var harness = new Harness();

        SetScope(CompanyScope.ForCompanies([CompanyA, CompanyB], active: null));

        var act = async () => await harness.Service.SetStringSettingAsync(
            "currency.code", "GHS", "General", "desc");
        await act.Should().ThrowAsync<AppSettingScopeViolationException>();

        (await harness.Db.AppSettings.IgnoreQueryFilters().ToListAsync()).Should().BeEmpty();
    }

    [Fact(DisplayName = "A write refused by the company guard surfaces as a failure instead of being reported as success")]
    public async Task RefusedCrossCompanyWrite_IsNotReportedAsSuccess()
    {
        using var harness = new Harness();

        SetScope(CompanyScope.ForCompanies([CompanyA], CompanyA));

        var foreign = AppSetting.Create("foreign.key", "x", "General");
        foreign.WithCompanyId("company-outside-scope");
        harness.Db.AppSettings.Add(foreign);

        var act = async () => await harness.Service.SetStringSettingAsync(
            "currency.code", "GHS", "General", "desc");
        await act.Should().ThrowAsync<CrossCompanyWriteException>();
    }

    [Fact(DisplayName = "A tenant-default write evicts every company's cached value, not just the writer's")]
    public async Task TenantDefaultWrite_EvictsOtherCompaniesCachedValues()
    {
        using var harness = new Harness();

        SetScope(CompanyScope.AllCompanies(active: null));
        await harness.Service.SetStringSettingAsync("leave.approvalLevels", "1", "Leave", "desc");

        SetScope(CompanyScope.ForCompanies([CompanyA, CompanyB], CompanyA));
        (await harness.Service.GetStringSettingAsync("leave.approvalLevels")).Should().Be("1");

        SetScope(CompanyScope.AllCompanies(active: null));
        await harness.Service.SetStringSettingAsync("leave.approvalLevels", "2", "Leave", "desc");

        SetScope(CompanyScope.ForCompanies([CompanyA, CompanyB], CompanyA));
        (await harness.Service.GetStringSettingAsync("leave.approvalLevels")).Should().Be("2");
    }

    [Fact(DisplayName = "Cache keys for the same key differ between two companies in the same tenant")]
    public async Task CacheKeys_DifferBetweenCompanies_InSameTenant()
    {
        using var harness = new Harness();

        SetScope(CompanyScope.ForCompanies([CompanyA, CompanyB], CompanyA));
        await harness.Service.SetStringSettingAsync("ui.theme", "Dark", "UI", "desc");
        await harness.Service.GetStringSettingAsync("ui.theme");

        SetScope(CompanyScope.ForCompanies([CompanyA, CompanyB], CompanyB));
        await harness.Service.SetStringSettingAsync("ui.theme", "Light", "UI", "desc");
        await harness.Service.GetStringSettingAsync("ui.theme");

        var keys = harness.Cache.Keys.Select(k => k.ToString()!)
            .Where(k => k.Contains("ui.theme"))
            .ToList();

        keys.Should().Contain(k => k.Contains(Tenant) && k.Contains(CompanyA));
        keys.Should().Contain(k => k.Contains(Tenant) && k.Contains(CompanyB));
        keys.Distinct().Should().HaveCount(2);
    }
}
