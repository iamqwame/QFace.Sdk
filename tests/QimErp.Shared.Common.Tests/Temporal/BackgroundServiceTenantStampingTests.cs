using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.Interceptors;
using QimErp.Shared.Common.Services.Auth;
using QimErp.Shared.Common.Services.MultiTenancy;
using Xunit;

namespace QimErp.Shared.Common.Tests.Temporal;

/// <summary>
/// Integration tests for the full background-service TenantId stamping chain:
///
///   Background service calls ICurrentUserService.SetContext(tenantId)
///       → calls DbContext.SaveChangesAsync()
///       → AuditEntitySaveChangesInterceptor reads GetTenantId() from AsyncLocal
///       → stamps TenantId on every Added entity automatically
///
/// These tests answer: "Does the EF Core interceptor actually stamp TenantId
/// when called from a non-HTTP context after SetContext is called?"
///
/// If ALL tests here pass AND all TenantContextActivityInterceptorTests pass,
/// it is safe to remove the manual WithTenantId() calls from activity code.
/// </summary>
public sealed class BackgroundServiceTenantStampingTests
{
    private const string TestTenantId = "019e31ec-test-bg-0000-000000000001";

    // ── DI factory ────────────────────────────────────────────────────────────

    private sealed class TestScope : IDisposable
    {
        private readonly ServiceProvider _root;
        private readonly IServiceScope _scope;

        public MinimalTestDbContext Db { get; }
        public UserContextService UserSvc { get; }

        public TestScope(string? dbName = null)
        {
            var services = new ServiceCollection();

            services.AddHttpContextAccessor();
            services.AddScoped<UserContextService>();
            services.AddScoped<ICurrentUserService>(sp => sp.GetRequiredService<UserContextService>());
            services.AddScoped<ITenantContext, TenantContext>();
            services.AddLogging();
            services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(NullLoggerFactory.Instance);
            services.AddScoped<AuditEntitySaveChangesInterceptor>();

            _root = services.BuildServiceProvider();
            _scope = _root.CreateScope();
            var sp = _scope.ServiceProvider;

            UserSvc = sp.GetRequiredService<UserContextService>();

            // Build DbContext options manually so the interceptor is resolved from the SAME
            // scope as UserContextService — avoiding EF Core's captive-dependency problem.
            var interceptor = sp.GetRequiredService<AuditEntitySaveChangesInterceptor>();
            // Use MinimalTestDbContext — only EntityCodeConfig, no Dictionary<> properties
            // that the in-memory provider cannot map.
            var options = new DbContextOptionsBuilder<MinimalTestDbContext>()
                .UseInMemoryDatabase(dbName ?? $"bgtest-{Guid.NewGuid()}")
                .AddInterceptors(interceptor)
                .Options;

            var tenantCtx = sp.GetRequiredService<ITenantContext>();
            Db = new MinimalTestDbContext(options, tenantCtx);
        }

        public void Dispose()
        {
            _scope.Dispose();
            _root.Dispose();
        }
    }

    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "TenantId stamped on entity when SetContext called before SaveChanges (background service path)")]
    public async Task TenantId_stamped_when_SetContext_called_before_save()
    {
        using var scope = new TestScope();

        // Simulate what TenantContextActivityInterceptor does at activity start
        scope.UserSvc.SetContext(TestTenantId, "bg-worker@system", "BG Worker", "system");

        var setting = EntityCodeConfig.Create(string.Empty, "BgTestEntity");
        scope.Db.EntityCodeConfigs.Add(setting);

        await scope.Db.SaveChangesAsync();

        setting.TenantId.Should().Be(TestTenantId,
            "AuditEntitySaveChangesInterceptor must read TenantId from AsyncLocal " +
            "and stamp it on every entity added in a non-HTTP background context");
    }

    [Fact(DisplayName = "Throws InvalidOperationException when SaveChanges called without any SetContext (missing ambient tenant)")]
    public async Task Throws_when_no_context_set_at_all()
    {
        using var scope = new TestScope();

        // NO SetContext call — bug where background service forgot to seed
        var setting = EntityCodeConfig.Create(string.Empty, "NoTenantEntity");
        scope.Db.EntityCodeConfigs.Add(setting);

        var act = async () => await scope.Db.SaveChangesAsync();

        await act.Should().ThrowAsync<InvalidOperationException>(
            "saving entities without a TenantId context must throw — " +
            "silent NULL rows are a data integrity failure");
    }

    [Fact(DisplayName = "After ClearContext, subsequent save without re-seeding throws")]
    public async Task After_ClearContext_save_without_reseeding_throws()
    {
        using var scope = new TestScope();

        // First save — works fine
        scope.UserSvc.SetContext(TestTenantId, "bg@system");
        var first = EntityCodeConfig.Create(string.Empty, "FirstEntity");
        scope.Db.EntityCodeConfigs.Add(first);
        await scope.Db.SaveChangesAsync();
        first.TenantId.Should().Be(TestTenantId);

        // Clear — simulates end of activity (ClearTenantContext() in finally block)
        scope.UserSvc.ClearContext();

        // Second save WITHOUT re-seeding — must throw
        var second = EntityCodeConfig.Create(string.Empty, "SecondEntity");
        scope.Db.EntityCodeConfigs.Add(second);

        var act = async () => await scope.Db.SaveChangesAsync();
        await act.Should().ThrowAsync<InvalidOperationException>(
            "after ClearContext, saving without calling SetContext again must throw");
    }

    [Fact(DisplayName = "CRITICAL: two concurrent background scopes stamp their own TenantIds — no bleed")]
    public async Task Concurrent_scopes_stamp_correct_tenants()
    {
        // Simulates two Temporal activities running at the same time in the same worker process.
        // Each gets its own DI scope (as Temporal .NET SDK does per activity).
        // AsyncLocal must isolate TenantId between them.

        const string tenantX = "tenant-x-concurrent";
        const string tenantY = "tenant-y-concurrent";

        var sharedDb = $"bgtest-concurrent-{Guid.NewGuid()}";
        Guid? idX = null;
        Guid? idY = null;

        async Task SimulateActivity(string tenantId, string key, Action<Guid> capture)
        {
            using var actScope = new TestScope(sharedDb);
            actScope.UserSvc.SetContext(tenantId, "bg@system");
            var entity = EntityCodeConfig.Create(string.Empty, $"Entity-{key}");
            actScope.Db.EntityCodeConfigs.Add(entity);
            await Task.Delay(20); // artificial overlap window so async chains interleave
            await actScope.Db.SaveChangesAsync();
            capture(entity.Id);
        }

        // Run both concurrently — their async continuations will overlap
        await Task.WhenAll(
            SimulateActivity(tenantX, "key-x", id => idX = id),
            SimulateActivity(tenantY, "key-y", id => idY = id));

        // Read back ignoring query filters to check the actual stored TenantId
        using var readScope = new TestScope(sharedDb);
        var x = await readScope.Db.EntityCodeConfigs.IgnoreQueryFilters().FirstAsync(a => a.Id == idX!.Value);
        var y = await readScope.Db.EntityCodeConfigs.IgnoreQueryFilters().FirstAsync(a => a.Id == idY!.Value);

        x.TenantId.Should().Be(tenantX, "scope X must stamp tenantX, not bleed tenantY");
        y.TenantId.Should().Be(tenantY, "scope Y must stamp tenantY, not bleed tenantX");
    }
}
