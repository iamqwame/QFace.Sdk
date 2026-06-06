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

        /// <summary>
        /// Original constructor for write-path tests — no tenantId seeding on ITenantContext.
        /// Tests that call SetContext manually continue to work unchanged.
        /// </summary>
        public TestScope(string? dbName = null)
            : this(tenantId: null, dbName: dbName, seedTenantContext: false)
        {
        }

        /// <summary>
        /// Constructor for read-path tests — seeds BOTH UserContextService AND ITenantContext
        /// so the EF global query filter is properly activated for the scope.
        /// Pass <paramref name="tenantId"/> = null to test the "no context" / empty-result path.
        /// </summary>
        public TestScope(string? tenantId, string? dbName)
            : this(tenantId: tenantId, dbName: dbName, seedTenantContext: true)
        {
        }

        private TestScope(string? tenantId, string? dbName, bool seedTenantContext)
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

            if (seedTenantContext && tenantId is not null)
            {
                // Seed UserContextService (write interceptor) AND ITenantContext (EF global filter).
                // This mirrors what TenantContextActivityInterceptor should do at activity start:
                // both the save-interceptor path and the read-filter path need the ambient tenant.
                UserSvc.SetContext(tenantId, "bg-worker@system", "BG Worker", "system");
                tenantCtx.SetTenant(tenantId);  // seeds EF global query filter
            }
            else if (seedTenantContext && tenantId is null)
            {
                // Explicitly set null — models the "no tenant context" scenario for the
                // empty-result safety test. TenantContext AsyncLocal defaults to null anyway,
                // but we call SetTenant(null) explicitly to document intent.
                tenantCtx.SetTenant(null);
            }
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

    // ── Read-path (EF global query filter) tests ──────────────────────────────
    // These verify that the EF global query filter (HasQueryFilter) properly scopes
    // reads when ITenantContext is seeded via SetTenant() — the counterpart to the
    // write-path tests above.
    //
    // TenantContextActivityInterceptor seeds ITenantScopeSetter (→ SetTenant) at activity
    // start; see TenantContextActivityInterceptorTests.Seeds_tenantScope_* for the
    // interceptor-level coverage. These tests verify the EF filter behaviour in isolation.

    [Fact(DisplayName = "Background: EF global query filter scopes reads to the correct tenant when ITenantContext is seeded")]
    public async Task Global_filter_scopes_reads_to_correct_tenant()
    {
        // Arrange: save two entities in two different tenants
        const string tenantA = "019e31ec-read-aaaa-0000-000000000001";
        const string tenantB = "019e31ec-read-bbbb-0000-000000000002";
        var sharedDb = $"bgread-{Guid.NewGuid()}";

        // Save entity for Tenant A
        using (var s = new TestScope(tenantA, sharedDb))
        {
            var e = EntityCodeConfig.Create(string.Empty, "TenantAEntity");
            s.Db.EntityCodeConfigs.Add(e);
            await s.Db.SaveChangesAsync();
        }

        // Save entity for Tenant B
        using (var s = new TestScope(tenantB, sharedDb))
        {
            var e = EntityCodeConfig.Create(string.Empty, "TenantBEntity");
            s.Db.EntityCodeConfigs.Add(e);
            await s.Db.SaveChangesAsync();
        }

        // Act: read as Tenant A — global filter should return ONLY TenantA's entities
        using var readScopeA = new TestScope(tenantA, sharedDb);
        var resultsA = await readScopeA.Db.EntityCodeConfigs.ToListAsync();

        // Assert
        resultsA.Should().HaveCount(1, "global filter must scope reads to tenantA only");
        resultsA[0].TenantId.Should().Be(tenantA);
        resultsA.Should().NotContain(e => e.TenantId == tenantB,
            "tenantB data must be invisible when reading as tenantA");
    }

    [Fact(DisplayName = "Background: EF global filter returns empty when no tenant context is set (not all-tenants leak)")]
    public async Task Global_filter_returns_empty_when_no_context()
    {
        // Arrange: save an entity with a known tenant
        const string tenant = "019e31ec-read-noread-0000-000000000001";
        var sharedDb = $"bgread-empty-{Guid.NewGuid()}";

        using (var s = new TestScope(tenant, sharedDb))
        {
            var e = EntityCodeConfig.Create(string.Empty, "SomeEntity");
            s.Db.EntityCodeConfigs.Add(e);
            await s.Db.SaveChangesAsync();
        }

        // Act: read with NO tenant context set — global filter gets null TenantId
        // Expected: returns empty (filter e.TenantId == null returns nothing for non-null rows)
        // This is SAFE behaviour — no tenant context = sees nothing (not all tenants)
        using var noCtxScope = new TestScope(tenantId: null, sharedDb);
        var results = await noCtxScope.Db.EntityCodeConfigs.ToListAsync();

        results.Should().BeEmpty(
            "with no tenant context, global filter (e.TenantId == null) must not leak any tenant's data");
    }

    [Fact(DisplayName = "Background: two concurrent scopes read only their own tenant's data (no cross-tenant leak via reads)")]
    public async Task Concurrent_scopes_read_only_own_tenant()
    {
        const string tenantX = "019e31ec-read-xxxx-0000-000000000001";
        const string tenantY = "019e31ec-read-yyyy-0000-000000000002";
        var sharedDb = $"bgread-concurrent-{Guid.NewGuid()}";

        // Seed both tenants
        using (var s = new TestScope(tenantX, sharedDb))
        {
            s.Db.EntityCodeConfigs.Add(EntityCodeConfig.Create(string.Empty, "EntityX"));
            await s.Db.SaveChangesAsync();
        }
        using (var s = new TestScope(tenantY, sharedDb))
        {
            s.Db.EntityCodeConfigs.Add(EntityCodeConfig.Create(string.Empty, "EntityY"));
            await s.Db.SaveChangesAsync();
        }

        List<EntityCodeConfig>? seenByX = null;
        List<EntityCodeConfig>? seenByY = null;

        async Task ReadAs(string tenantId, Action<List<EntityCodeConfig>> capture)
        {
            using var scope = new TestScope(tenantId, sharedDb);
            await Task.Delay(10); // force overlap
            var results = await scope.Db.EntityCodeConfigs.ToListAsync();
            capture(results);
        }

        await Task.WhenAll(
            ReadAs(tenantX, r => seenByX = r),
            ReadAs(tenantY, r => seenByY = r));

        seenByX.Should().HaveCount(1).And.OnlyContain(e => e.TenantId == tenantX,
            "scope X must only see tenantX data");
        seenByY.Should().HaveCount(1).And.OnlyContain(e => e.TenantId == tenantY,
            "scope Y must only see tenantY data");
    }

    // ── IgnoreQueryFilters tests ──────────────────────────────────────────────
    // These verify that IgnoreQueryFilters() correctly bypasses the global tenant
    // filter, which is required for admin/audit cross-tenant queries and for the
    // existing concurrent-scope stamping test above.

    [Fact(DisplayName = "IgnoreQueryFilters bypasses tenant filter — sees ALL tenants' data")]
    public async Task IgnoreQueryFilters_bypasses_tenant_isolation()
    {
        const string tenantA = "019e31ec-ignore-aaaa-0000-000000000001";
        const string tenantB = "019e31ec-ignore-bbbb-0000-000000000002";
        var sharedDb = $"bgignore-{Guid.NewGuid()}";

        // Save one row for each tenant
        using (var s = new TestScope(tenantA, sharedDb))
        {
            s.Db.EntityCodeConfigs.Add(EntityCodeConfig.Create(string.Empty, "EntityA"));
            await s.Db.SaveChangesAsync();
        }
        using (var s = new TestScope(tenantB, sharedDb))
        {
            s.Db.EntityCodeConfigs.Add(EntityCodeConfig.Create(string.Empty, "EntityB"));
            await s.Db.SaveChangesAsync();
        }

        // Read AS tenantA — normal query should see only A
        using var readScope = new TestScope(tenantA, sharedDb);
        var normal = await readScope.Db.EntityCodeConfigs.ToListAsync();
        normal.Should().HaveCount(1, "normal query must respect global filter — one tenant only");

        // Read with IgnoreQueryFilters — should see BOTH tenants
        var ignored = await readScope.Db.EntityCodeConfigs.IgnoreQueryFilters().ToListAsync();
        ignored.Should().HaveCount(2, "IgnoreQueryFilters must bypass the global filter and see all tenants");
        ignored.Select(e => e.TenantId).Should().Contain(tenantA).And.Contain(tenantB,
            "both tenants must be visible when the filter is explicitly ignored");
    }

    [Fact(DisplayName = "IgnoreQueryFilters with no tenant context still sees all data (audit/admin use case)")]
    public async Task IgnoreQueryFilters_with_no_context_sees_all_data()
    {
        const string tenantX = "019e31ec-ignore-xxxx-0000-000000000001";
        var sharedDb = $"bgignore-noCtx-{Guid.NewGuid()}";

        // Save a row for tenantX
        using (var s = new TestScope(tenantX, sharedDb))
        {
            s.Db.EntityCodeConfigs.Add(EntityCodeConfig.Create(string.Empty, "EntityX"));
            await s.Db.SaveChangesAsync();
        }

        // Read with no context + IgnoreQueryFilters (e.g. admin/audit cross-tenant query)
        using var noCtx = new TestScope(tenantId: null, sharedDb);
        var result = await noCtx.Db.EntityCodeConfigs.IgnoreQueryFilters().ToListAsync();
        result.Should().HaveCount(1, "IgnoreQueryFilters with no context must still return the data");
        result[0].TenantId.Should().Be(tenantX);
    }

    [Fact(DisplayName = "Full cycle: write → clear context → read own data only → IgnoreQueryFilters sees more")]
    public async Task Full_write_read_ignore_cycle()
    {
        const string tenant1 = "019e31ec-cycle-1111-0000-000000000001";
        const string tenant2 = "019e31ec-cycle-2222-0000-000000000002";
        var sharedDb = $"bgcycle-{Guid.NewGuid()}";

        // --- WRITE: both tenants save a row ---
        using (var s1 = new TestScope(tenant1, sharedDb))
        {
            s1.Db.EntityCodeConfigs.Add(EntityCodeConfig.Create(string.Empty, "Cycle-T1"));
            await s1.Db.SaveChangesAsync();
        }
        using (var s2 = new TestScope(tenant2, sharedDb))
        {
            s2.Db.EntityCodeConfigs.Add(EntityCodeConfig.Create(string.Empty, "Cycle-T2"));
            await s2.Db.SaveChangesAsync();
        }

        // --- READ: as tenant1, global filter sees only tenant1 ---
        using var readScope = new TestScope(tenant1, sharedDb);
        var filtered = await readScope.Db.EntityCodeConfigs.ToListAsync();
        filtered.Should().HaveCount(1).And.OnlyContain(e => e.TenantId == tenant1,
            "filtered read must only see own tenant");

        // --- IGNORE: bypasses filter, sees both ---
        var unfiltered = await readScope.Db.EntityCodeConfigs.IgnoreQueryFilters().ToListAsync();
        unfiltered.Should().HaveCount(2, "IgnoreQueryFilters must see both tenants");

        // --- VERIFY: the ignored result contains both tenants' data ---
        unfiltered.Should().Contain(e => e.TenantId == tenant1, "must contain tenant1 data");
        unfiltered.Should().Contain(e => e.TenantId == tenant2, "must contain tenant2 data even though we're scoped to tenant1");
    }
}
