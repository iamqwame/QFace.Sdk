using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.ExceptionHandlers;
using QimErp.Shared.Common.Interceptors;
using QimErp.Shared.Common.Services;
using QimErp.Shared.Common.Services.Auth;
using QimErp.Shared.Common.Services.MultiTenancy;
using Xunit;

namespace QimErp.Shared.Common.Tests.MultiTenancy;

public sealed class CompanyStampInterceptorTests : IDisposable
{
    private const string Tenant = "019e31ec-comp-0000-0000-000000000001";
    private const string CompanyA = "company-a";
    private const string CompanyB = "company-b";

    private sealed class TenantWideRow : GuidAuditableEntity, ITenantWideEntity
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class CompanyDbContext(DbContextOptions<CompanyDbContext> options) : DbContext(options)
    {
        public DbSet<EntityCodeConfig> Configs { get; set; } = null!;
        public DbSet<TenantWideRow> TenantWideRows { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<EntityCodeConfig>().Ignore(e => e.CustomFields);
            modelBuilder.Entity<TenantWideRow>().Ignore(e => e.CustomFields);
        }
    }

    private sealed class CapturingLogger : ILogger<AuditEntitySaveChangesInterceptor>
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => new NoopScope();
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
            => Entries.Add((logLevel, formatter(state, exception)));

        private sealed class NoopScope : IDisposable
        {
            public void Dispose() { }
        }
    }

    private sealed class Harness : IDisposable
    {
        private readonly ServiceProvider _root;
        private readonly IServiceScope _scope;

        public CompanyDbContext Db { get; }
        public CapturingLogger Log { get; } = new();

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
            sp.GetRequiredService<ITenantContext>().SetTenant(Tenant);

            var interceptor = new AuditEntitySaveChangesInterceptor(userService, Log, sp);

            Db = new CompanyDbContext(new DbContextOptionsBuilder<CompanyDbContext>()
                .UseInMemoryDatabase($"company-{Guid.NewGuid()}")
                .AddInterceptors(interceptor)
                .Options);
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

    [Fact(DisplayName = "Active company is the write target")]
    public async Task ActiveCompany_IsStamped()
    {
        using var harness = new Harness();
        SetScope(CompanyScope.ForCompanies([CompanyA, CompanyB], CompanyA));

        var row = EntityCodeConfig.Create(string.Empty, "Invoice");
        harness.Db.Configs.Add(row);
        await harness.Db.SaveChangesAsync();

        row.CompanyId.Should().Be(CompanyA);
    }

    [Fact(DisplayName = "A single real company in scope is inferred without a header")]
    public async Task SingleCompanyInScope_IsInferred()
    {
        using var harness = new Harness();
        SetScope(CompanyScope.ForCompanies([CompanyA], active: null));

        var row = EntityCodeConfig.Create(string.Empty, "Invoice");
        harness.Db.Configs.Add(row);
        await harness.Db.SaveChangesAsync();

        row.CompanyId.Should().Be(CompanyA);
    }

    [Fact(DisplayName = "Ambiguous multi-company scope throws and names the entity type")]
    public async Task AmbiguousScope_Throws()
    {
        using var harness = new Harness();
        SetScope(CompanyScope.ForCompanies([CompanyA, CompanyB], active: null));

        harness.Db.Configs.Add(EntityCodeConfig.Create(string.Empty, "Invoice"));

        var act = async () => await harness.Db.SaveChangesAsync();

        var thrown = await act.Should().ThrowAsync<InvalidOperationException>();
        thrown.Which.Message.Should().Contain(nameof(EntityCodeConfig))
            .And.Contain("X-Company-Id")
            .And.Contain("WithCompanyId")
            .And.Contain("AsTenantShared")
            .And.Contain("CompanyStampScope.Enter");
    }

    [Fact(DisplayName = "MultiCompanyEnabled=false stamps \"\" and never throws")]
    public async Task FlagOff_StampsShared_NeverThrows()
    {
        using var harness = new Harness();
        SetScope(CompanyScope.Inactive);

        var row = EntityCodeConfig.Create(string.Empty, "Invoice");
        harness.Db.Configs.Add(row);

        var act = async () => await harness.Db.SaveChangesAsync();

        await act.Should().NotThrowAsync();
        row.CompanyId.Should().BeEmpty();
    }

    [Fact(DisplayName = "ITenantWideEntity is never company-stamped")]
    public async Task TenantWideEntity_IsNeverStamped()
    {
        using var harness = new Harness();
        SetScope(CompanyScope.ForCompanies([CompanyA], CompanyA));

        var row = new TenantWideRow { Name = "Currency" };
        harness.Db.TenantWideRows.Add(row);
        await harness.Db.SaveChangesAsync();

        row.CompanyId.Should().BeEmpty();
    }

    [Fact(DisplayName = "CompanyStampScope.Enter supplies the write target when no company is active")]
    public async Task CompanyStampScope_Enter_Overrides()
    {
        using var harness = new Harness();
        SetScope(CompanyScope.AllCompanies(active: null));

        var row = EntityCodeConfig.Create(string.Empty, "Invoice");
        harness.Db.Configs.Add(row);

        using (CompanyStampScope.Enter(CompanyB))
        {
            await harness.Db.SaveChangesAsync();
        }

        row.CompanyId.Should().Be(CompanyB);
    }

    [Fact(DisplayName = "CompanyStampScope.EnterShared stamps \"\"")]
    public async Task CompanyStampScope_EnterShared_StampsShared()
    {
        using var harness = new Harness();
        SetScope(CompanyScope.AllCompanies(active: null));

        var row = EntityCodeConfig.Create(string.Empty, "Invoice");
        harness.Db.Configs.Add(row);

        using (CompanyStampScope.EnterShared())
        {
            await harness.Db.SaveChangesAsync();
        }

        row.CompanyId.Should().BeEmpty();
    }

    [Fact(DisplayName = "BulkSeedScope stamps \"\" and logs a warning instead of throwing")]
    public async Task BulkSeedScope_StampsShared_WithWarning()
    {
        using var harness = new Harness();
        SetScope(CompanyScope.AllCompanies(active: null));

        var row = EntityCodeConfig.Create(string.Empty, "Invoice");
        harness.Db.Configs.Add(row);

        using (BulkSeedScope.Enter())
        {
            await harness.Db.SaveChangesAsync();
        }

        row.CompanyId.Should().BeEmpty();
        harness.Log.Entries.Should().Contain(e =>
            e.Level == LogLevel.Warning && e.Message.Contains("BulkSeedScope active with no company write target"));
    }

    [Fact(DisplayName = "Guard throws when the ORIGINAL company is outside scope")]
    public async Task Guard_Throws_When_OriginalCompany_OutOfScope()
    {
        using var harness = new Harness();
        SetScope(CompanyScope.ForCompanies([CompanyA, CompanyB], CompanyB));

        var row = EntityCodeConfig.Create(string.Empty, "Invoice");
        harness.Db.Configs.Add(row);
        await harness.Db.SaveChangesAsync();
        row.CompanyId.Should().Be(CompanyB);

        SetScope(CompanyScope.ForCompanies([CompanyA], CompanyA));

        // The IgnoreQueryFilters() re-stamp: CurrentValue looks legitimate, OriginalValue does not.
        row.CompanyId = CompanyA;

        var act = async () => await harness.Db.SaveChangesAsync();
        await act.Should().ThrowAsync<CrossCompanyWriteException>();
    }

    [Fact(DisplayName = "Guard allows an in-scope company move and logs a warning")]
    public async Task Guard_Allows_InScope_Move()
    {
        using var harness = new Harness();
        SetScope(CompanyScope.ForCompanies([CompanyA, CompanyB], CompanyA));

        var row = EntityCodeConfig.Create(string.Empty, "Invoice");
        harness.Db.Configs.Add(row);
        await harness.Db.SaveChangesAsync();

        row.CompanyId = CompanyB;

        var act = async () => await harness.Db.SaveChangesAsync();

        await act.Should().NotThrowAsync();
        harness.Log.Entries.Should().Contain(e =>
            e.Level == LogLevel.Warning && e.Message.Contains("Cross-company move"));
    }

    [Fact(DisplayName = "Synchronous SaveChanges stamps tenant and company identically to the async path")]
    public void SyncSaveChanges_StampsTenantAndCompany()
    {
        using var harness = new Harness();
        SetScope(CompanyScope.ForCompanies([CompanyA, CompanyB], CompanyA));

        var row = EntityCodeConfig.Create(string.Empty, "Invoice");
        harness.Db.Configs.Add(row);
        harness.Db.SaveChanges();

        row.TenantId.Should().Be(Tenant);
        row.CompanyId.Should().Be(CompanyA);
    }

    [Fact(DisplayName = "Synchronous SaveChanges runs the cross-company guard")]
    public void SyncSaveChanges_RunsGuard()
    {
        using var harness = new Harness();
        SetScope(CompanyScope.ForCompanies([CompanyA, CompanyB], CompanyB));

        var row = EntityCodeConfig.Create(string.Empty, "Invoice");
        harness.Db.Configs.Add(row);
        harness.Db.SaveChanges();

        SetScope(CompanyScope.ForCompanies([CompanyA], CompanyA));
        harness.Db.Entry(row).State = EntityState.Modified;

        var act = () => harness.Db.SaveChanges();
        act.Should().Throw<CrossCompanyWriteException>();
    }

    [Fact(DisplayName = "Editing a tenant-shared row never stamps it with the active company")]
    public async Task Modified_TenantSharedRow_IsNotStamped()
    {
        using var harness = new Harness();
        SetScope(CompanyScope.AllCompanies(active: null));

        var row = EntityCodeConfig.Create(string.Empty, "Invoice");
        harness.Db.Configs.Add(row);
        using (CompanyStampScope.EnterShared())
        {
            await harness.Db.SaveChangesAsync();
        }

        row.CompanyId.Should().BeEmpty();

        SetScope(CompanyScope.ForCompanies([CompanyA], CompanyA));
        harness.Db.Entry(row).State = EntityState.Modified;
        await harness.Db.SaveChangesAsync();

        row.CompanyId.Should().BeEmpty();
        harness.Log.Entries.Should().NotContain(e => e.Message.Contains("Cross-company move"));
    }

    [Fact(DisplayName = "A tenant-shared row edited under one company stays writable from another")]
    public async Task Modified_TenantSharedRow_StaysWritable_FromAnotherCompany()
    {
        using var harness = new Harness();
        SetScope(CompanyScope.AllCompanies(active: null));

        var row = EntityCodeConfig.Create(string.Empty, "Invoice");
        harness.Db.Configs.Add(row);
        using (CompanyStampScope.EnterShared())
        {
            await harness.Db.SaveChangesAsync();
        }

        SetScope(CompanyScope.ForCompanies([CompanyA], CompanyA));
        harness.Db.Entry(row).State = EntityState.Modified;
        await harness.Db.SaveChangesAsync();

        SetScope(CompanyScope.ForCompanies([CompanyB], CompanyB));
        harness.Db.Entry(row).State = EntityState.Modified;

        var act = async () => await harness.Db.SaveChangesAsync();

        await act.Should().NotThrowAsync();
        row.CompanyId.Should().BeEmpty();
    }

    [Fact(DisplayName = "An added row is still stamped when batched with a modified tenant-shared row")]
    public async Task Added_IsStillStamped_WhenBatchedWithModifiedSharedRow()
    {
        using var harness = new Harness();
        SetScope(CompanyScope.AllCompanies(active: null));

        var shared = EntityCodeConfig.Create(string.Empty, "Invoice");
        harness.Db.Configs.Add(shared);
        using (CompanyStampScope.EnterShared())
        {
            await harness.Db.SaveChangesAsync();
        }

        SetScope(CompanyScope.ForCompanies([CompanyA], CompanyA));
        harness.Db.Entry(shared).State = EntityState.Modified;

        var added = EntityCodeConfig.Create(string.Empty, "Receipt");
        harness.Db.Configs.Add(added);

        await harness.Db.SaveChangesAsync();

        shared.CompanyId.Should().BeEmpty();
        added.CompanyId.Should().Be(CompanyA);
    }
}
