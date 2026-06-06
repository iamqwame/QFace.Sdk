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
/// Edge-case tests for <see cref="AuditEntitySaveChangesInterceptor"/>.
///
/// Every path through SetTenantIdOnEntities is covered:
///
///   Path A — no AuditableEntity rows in batch → skip silently (IAM login / IdentityUser saves)
///   Path B — AuditableEntity rows already have TenantId → propagate to siblings that don't
///   Path C — AuditableEntity has no TenantId, non-AuditableEntity sibling has one → inherit
///   Path D — nothing has TenantId and ambient context is empty → throw
///   Path E — ambient context (HTTP/background) provides TenantId → stamp all blank entries
///   Path F — mixed batch: some AuditableEntity rows stamped, some not → propagate from stamped
///
/// These tests directly reproduce the IAM login HTTP-500 bug (Path A / Path C) and
/// every other scenario that caused or could cause that error in future.
/// </summary>
public sealed class AuditEntitySaveChangesInterceptorEdgeCaseTests
{
    private const string TenantX = "019e31ec-edge-xxxx-0000-000000000001";
    private const string TenantY = "019e31ec-edge-yyyy-0000-000000000002";

    // ── Fake non-AuditableEntity that mimics IdentityUser / UserIdentity ──────

    /// <summary>
    /// Simulates IAM's <c>UserIdentity : IdentityUser</c> — has a TenantId property but
    /// does NOT extend AuditableEntity. The interceptor must NOT throw when the batch
    /// contains only rows of this type, and must inherit TenantId from it for sibling
    /// AuditableEntity rows (like UserToken).
    /// </summary>
    private sealed class FakeIdentityUser
    {
        public Guid   Id       { get; set; } = Guid.NewGuid();
        public string TenantId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
    }

    // ── Minimal DbContext that holds both types ───────────────────────────────

    private sealed class EdgeCaseDbContext(
        DbContextOptions<EdgeCaseDbContext> options,
        ITenantContext tenantContext)
        : DbContext(options)
    {
        public DbSet<EntityCodeConfig>  AuditableEntities  { get; set; } = null!;
        public DbSet<FakeIdentityUser>  IdentityUsers      { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // AuditableEntity — tenant-scoped global filter
            modelBuilder.Entity<EntityCodeConfig>()
                .Ignore(e => e.CustomFields)
                .HasQueryFilter(e => e.TenantId == tenantContext.TenantId);

            // FakeIdentityUser — no global filter (mirrors AuthApplicationDbContext)
            modelBuilder.Entity<FakeIdentityUser>()
                .HasKey(u => u.Id);
        }
    }

    // ── DI factory ────────────────────────────────────────────────────────────

    private sealed class EdgeScope : IDisposable
    {
        private readonly ServiceProvider _root;
        private readonly IServiceScope   _scope;

        public EdgeCaseDbContext Db       { get; }
        public UserContextService UserSvc { get; }
        public ITenantContext     TenantCtx { get; }

        public EdgeScope(string? tenantId = null, string? dbName = null)
        {
            var services = new ServiceCollection();
            services.AddHttpContextAccessor();
            services.AddScoped<UserContextService>();
            services.AddScoped<ICurrentUserService>(sp => sp.GetRequiredService<UserContextService>());
            services.AddScoped<ITenantContext, TenantContext>();
            services.AddLogging();
            services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(NullLoggerFactory.Instance);
            services.AddScoped<AuditEntitySaveChangesInterceptor>();

            _root  = services.BuildServiceProvider();
            _scope = _root.CreateScope();
            var sp = _scope.ServiceProvider;

            UserSvc   = sp.GetRequiredService<UserContextService>();
            TenantCtx = sp.GetRequiredService<ITenantContext>();

            var interceptor = sp.GetRequiredService<AuditEntitySaveChangesInterceptor>();
            var opts = new DbContextOptionsBuilder<EdgeCaseDbContext>()
                .UseInMemoryDatabase(dbName ?? $"edge-{Guid.NewGuid()}")
                .AddInterceptors(interceptor)
                .Options;

            Db = new EdgeCaseDbContext(opts, TenantCtx);

            if (!string.IsNullOrEmpty(tenantId))
            {
                UserSvc.SetContext(tenantId, "test@system");
                TenantCtx.SetTenant(tenantId);
            }
        }

        public void Dispose() { _scope.Dispose(); _root.Dispose(); }
    }

    // ── PATH A — No AuditableEntity in batch → skip silently ─────────────────

    [Fact(DisplayName = "PATH A: Saving only non-AuditableEntity rows (IdentityUser) with no tenant context does NOT throw")]
    public async Task PathA_Only_NonAuditable_No_Context_DoesNotThrow()
    {
        // Reproduces the IAM HTTP-500 bug:
        // LoginFeature calls userManager.UpdateAsync(userToUpdate) — saves UserIdentity
        // (extends IdentityUser, not AuditableEntity) with no JWT present.
        using var scope = new EdgeScope(tenantId: null);

        scope.Db.IdentityUsers.Add(new FakeIdentityUser
        {
            TenantId = TenantX,
            UserName = "test@login.com"
        });

        var act = async () => await scope.Db.SaveChangesAsync();

        await act.Should().NotThrowAsync(
            "saving non-AuditableEntity rows (e.g. IdentityUser) with no ambient tenant " +
            "must not throw — the global query filter does not apply to these types");
    }

    [Fact(DisplayName = "PATH A: Mixed batch — non-AuditableEntity rows only, no context, no throw")]
    public async Task PathA_MultipleNonAuditable_NoContext_NoThrow()
    {
        using var scope = new EdgeScope(tenantId: null);

        scope.Db.IdentityUsers.Add(new FakeIdentityUser { TenantId = TenantX, UserName = "a@test.com" });
        scope.Db.IdentityUsers.Add(new FakeIdentityUser { TenantId = TenantX, UserName = "b@test.com" });

        var act = async () => await scope.Db.SaveChangesAsync();
        await act.Should().NotThrowAsync();
    }

    // ── PATH B — AuditableEntity already has TenantId → propagate to siblings ─

    [Fact(DisplayName = "PATH B: Stamped AuditableEntity propagates TenantId to unstamped sibling in same batch")]
    public async Task PathB_StampedSibling_PropagatesTo_UnstampedSibling()
    {
        // One entity already has TenantId set (e.g. loaded from DB).
        // A second entity in the same batch was just created with blank TenantId.
        // The interceptor must copy TenantId from the stamped one to the blank one.
        using var scope = new EdgeScope(tenantId: null); // no ambient context

        var already = EntityCodeConfig.Create(string.Empty, "AlreadyStamped");
        already.WithTenantId(TenantX); // explicitly stamped

        var blank = EntityCodeConfig.Create(string.Empty, "BlankTenant");
        // blank.TenantId == "" intentionally

        scope.Db.AuditableEntities.Add(already);
        scope.Db.AuditableEntities.Add(blank);
        await scope.Db.SaveChangesAsync();

        blank.TenantId.Should().Be(TenantX,
            "interceptor must propagate TenantId from explicitly-stamped sibling to blank sibling");
        already.TenantId.Should().Be(TenantX, "already-stamped entity must keep its own TenantId");
    }

    // ── PATH C — Non-AuditableEntity sibling provides TenantId ───────────────

    [Fact(DisplayName = "PATH C: AuditableEntity (UserToken) inherits TenantId from non-AuditableEntity sibling (UserIdentity) — the exact IAM login scenario")]
    public async Task PathC_AuditableEntity_InheritsFrom_NonAuditableSibling()
    {
        // The exact scenario that caused HTTP 500 on login:
        // userManager.UpdateAsync saves UserToken (AuditableEntity, blank TenantId)
        // + UserIdentity (IdentityUser, has TenantId) in the same SaveChanges.
        using var scope = new EdgeScope(tenantId: null); // unauthenticated — no JWT

        // Simulates UserIdentity (already exists in DB with a TenantId)
        var identityUser = new FakeIdentityUser { TenantId = TenantX, UserName = "login@test.com" };

        // Simulates UserToken / refresh token (just created, TenantId not yet set)
        var refreshToken = EntityCodeConfig.Create(string.Empty, "RefreshToken");
        // refreshToken.TenantId == "" intentionally — will be inherited

        scope.Db.IdentityUsers.Add(identityUser);
        scope.Db.AuditableEntities.Add(refreshToken);

        var act = async () => await scope.Db.SaveChangesAsync();
        await act.Should().NotThrowAsync(
            "AuditableEntity must inherit TenantId from non-AuditableEntity sibling in same batch");

        refreshToken.TenantId.Should().Be(TenantX,
            "UserToken must get TenantId from the UserIdentity saved in the same SaveChanges call");
    }

    [Fact(DisplayName = "PATH C: Only non-AuditableEntity has TenantId — AuditableEntity with blank TenantId gets it")]
    public async Task PathC_OnlyNonAuditable_HasTenantId_Propagated()
    {
        using var scope = new EdgeScope(tenantId: null);

        var user    = new FakeIdentityUser { TenantId = TenantY, UserName = "x@test.com" };
        var audited = EntityCodeConfig.Create(string.Empty, "AuditedEntity");

        scope.Db.IdentityUsers.Add(user);
        scope.Db.AuditableEntities.Add(audited);
        await scope.Db.SaveChangesAsync();

        audited.TenantId.Should().Be(TenantY);
    }

    // ── PATH D — Nothing has TenantId → throw ────────────────────────────────

    [Fact(DisplayName = "PATH D: AuditableEntity with no context and no sibling TenantId throws InvalidOperationException")]
    public async Task PathD_NoContext_NoSibling_Throws()
    {
        // The scenario: a developer writes a background job that saves an AuditableEntity
        // but forgets to call SetContext / register TenantContextActivityInterceptor.
        // The interceptor must throw rather than silently save a NULL-TenantId row.
        using var scope = new EdgeScope(tenantId: null);

        scope.Db.AuditableEntities.Add(EntityCodeConfig.Create(string.Empty, "OrphanEntity"));

        var act = async () => await scope.Db.SaveChangesAsync();

        await act.Should().ThrowAsync<InvalidOperationException>(
            "saving AuditableEntity with no ambient tenant context and no sibling TenantId " +
            "must throw — a NULL TenantId row is a silent data integrity failure");
    }

    [Fact(DisplayName = "PATH D: Multiple AuditableEntity rows, all blank TenantId, no context → throws")]
    public async Task PathD_MultipleAuditable_AllBlank_NoContext_Throws()
    {
        using var scope = new EdgeScope(tenantId: null);

        scope.Db.AuditableEntities.Add(EntityCodeConfig.Create(string.Empty, "E1"));
        scope.Db.AuditableEntities.Add(EntityCodeConfig.Create(string.Empty, "E2"));

        var act = async () => await scope.Db.SaveChangesAsync();
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact(DisplayName = "PATH D: Non-AuditableEntity without TenantId + AuditableEntity with no context → throws")]
    public async Task PathD_NonAuditable_NoTenant_Plus_Auditable_Throws()
    {
        // Non-AuditableEntity has empty TenantId AND AuditableEntity is blank.
        // Neither fallback works → must throw.
        using var scope = new EdgeScope(tenantId: null);

        scope.Db.IdentityUsers.Add(new FakeIdentityUser { TenantId = "", UserName = "empty@test.com" });
        scope.Db.AuditableEntities.Add(EntityCodeConfig.Create(string.Empty, "OrphanAudited"));

        var act = async () => await scope.Db.SaveChangesAsync();
        await act.Should().ThrowAsync<InvalidOperationException>(
            "when non-AuditableEntity sibling also has empty TenantId the interceptor must throw");
    }

    // ── PATH E — Ambient context stamps everything ────────────────────────────

    [Fact(DisplayName = "PATH E: Ambient context (background SetContext) stamps all blank AuditableEntity rows")]
    public async Task PathE_AmbientContext_StampsAllBlankEntries()
    {
        using var scope = new EdgeScope(TenantX);

        scope.Db.AuditableEntities.Add(EntityCodeConfig.Create(string.Empty, "E1"));
        scope.Db.AuditableEntities.Add(EntityCodeConfig.Create(string.Empty, "E2"));
        await scope.Db.SaveChangesAsync();

        var all = await scope.Db.AuditableEntities.IgnoreQueryFilters().ToListAsync();
        all.Should().HaveCount(2).And.OnlyContain(e => e.TenantId == TenantX,
            "ambient context must stamp all blank AuditableEntity rows in the batch");
    }

    [Fact(DisplayName = "PATH E: Ambient context does NOT overwrite AuditableEntity that already has TenantId")]
    public async Task PathE_AmbientContext_DoesNotOverwrite_AlreadyStamped()
    {
        // If an entity was explicitly stamped with TenantY but the ambient context is TenantX,
        // the interceptor must NOT overwrite the explicit stamp.
        using var scope = new EdgeScope(TenantX);

        var explicit_ = EntityCodeConfig.Create(string.Empty, "ExplicitTenant");
        explicit_.WithTenantId(TenantY); // explicit override

        scope.Db.AuditableEntities.Add(explicit_);
        await scope.Db.SaveChangesAsync();

        explicit_.TenantId.Should().Be(TenantY,
            "explicitly-stamped TenantId must not be overwritten by the ambient context");
    }

    // ── PATH F — Mixed batch: some stamped, some blank ────────────────────────

    [Fact(DisplayName = "PATH F: Mixed batch — stamped entities propagate TenantId to blank siblings in same SaveChanges")]
    public async Task PathF_Mixed_Stamped_And_Blank_PropagatesCorrectly()
    {
        using var scope = new EdgeScope(tenantId: null); // no ambient context

        var stamped1 = EntityCodeConfig.Create(string.Empty, "Stamped1");
        stamped1.WithTenantId(TenantX);

        var blank1 = EntityCodeConfig.Create(string.Empty, "Blank1");
        var blank2 = EntityCodeConfig.Create(string.Empty, "Blank2");

        var stamped2 = EntityCodeConfig.Create(string.Empty, "Stamped2");
        stamped2.WithTenantId(TenantX);

        scope.Db.AuditableEntities.AddRange(stamped1, blank1, blank2, stamped2);
        await scope.Db.SaveChangesAsync();

        blank1.TenantId.Should().Be(TenantX, "blank entity 1 must inherit TenantX from stamped sibling");
        blank2.TenantId.Should().Be(TenantX, "blank entity 2 must inherit TenantX from stamped sibling");
        stamped1.TenantId.Should().Be(TenantX, "stamped entity 1 must keep TenantX");
        stamped2.TenantId.Should().Be(TenantX, "stamped entity 2 must keep TenantX");
    }

    [Fact(DisplayName = "PATH F: Mixed batch — only non-AuditableEntity stamped, all AuditableEntity blank → inherit from non-auditable")]
    public async Task PathF_NonAuditable_StampedOnly_AuditableInherits()
    {
        using var scope = new EdgeScope(tenantId: null);

        var identityUser  = new FakeIdentityUser { TenantId = TenantX, UserName = "mixed@test.com" };
        var auditedBlank1 = EntityCodeConfig.Create(string.Empty, "AuditedBlank1");
        var auditedBlank2 = EntityCodeConfig.Create(string.Empty, "AuditedBlank2");

        scope.Db.IdentityUsers.Add(identityUser);
        scope.Db.AuditableEntities.AddRange(auditedBlank1, auditedBlank2);
        await scope.Db.SaveChangesAsync();

        auditedBlank1.TenantId.Should().Be(TenantX);
        auditedBlank2.TenantId.Should().Be(TenantX);
    }

    // ── REGRESSION — the exact HTTP-500 login scenario end to end ─────────────

    [Fact(DisplayName = "REGRESSION: Login scenario — UserIdentity update + UserToken creation with no JWT does not throw")]
    public async Task Regression_Login_UserIdentityUpdate_UserTokenCreate_NoJwt()
    {
        // Full reproduction of the bug:
        //   POST /auth/login (no JWT)
        //   → LoginFeature.UpdateAsync(user) saves:
        //       - UserIdentity row (Modified, has TenantId from DB)
        //       - UserToken row (Added, blank TenantId — refresh token just created)
        using var scope = new EdgeScope(tenantId: null); // unauthenticated

        // Simulate an existing UserIdentity loaded from DB (already has TenantId)
        var existingUser = new FakeIdentityUser
        {
            Id       = Guid.NewGuid(),
            TenantId = TenantX,          // comes from DB — set when user was registered
            UserName = "user@techlabs.com"
        };

        // Simulate a new UserToken (refresh token) with blank TenantId — created during login
        var refreshToken = EntityCodeConfig.Create(string.Empty, "REFRESH_TOKEN_PREFIX");

        scope.Db.IdentityUsers.Add(existingUser);
        scope.Db.AuditableEntities.Add(refreshToken);

        // This must NOT throw (was throwing HTTP 500 before the fix)
        var act = async () => await scope.Db.SaveChangesAsync();
        await act.Should().NotThrowAsync("login flow must succeed even with no JWT on the request");

        // And the refresh token must have inherited TenantId from the UserIdentity
        refreshToken.TenantId.Should().Be(TenantX,
            "refresh token (UserToken / AuditableEntity) must inherit TenantId from UserIdentity in same SaveChanges");
    }

    [Fact(DisplayName = "REGRESSION: After login fix, existing background-job strict throw is NOT broken")]
    public async Task Regression_BackgroundJob_StrictThrow_StillWorks()
    {
        // Verify the fix didn't accidentally weaken the strict throw for legitimate violations.
        // A background service that saves AuditableEntity without SetContext → still throws.
        using var scope = new EdgeScope(tenantId: null);

        scope.Db.AuditableEntities.Add(EntityCodeConfig.Create(string.Empty, "UnscopedEntity"));

        var act = async () => await scope.Db.SaveChangesAsync();
        await act.Should().ThrowAsync<InvalidOperationException>(
            "the login fix must not weaken the strict throw for background services " +
            "that forget to set tenant context before saving AuditableEntity rows");
    }

    // ── SCOPING BUGS — tests that would have caught the LoginFeature bug ──────
    //
    // The IAM login 500 bug was caused by SetContext being scoped too narrowly:
    //
    //   SetContext()          ← context alive
    //   try { Save1() }       ← works
    //   finally { ClearContext() }  ← context killed HERE
    //   Save2()               ← throws 500  ← BUG: context already gone
    //
    // These tests verify that any code following this pattern is caught.
    // They would have caught the LoginFeature bug if they had existed first.

    [Fact(DisplayName = "SCOPING: SetContext scoped too narrowly — first save works, second save after ClearContext throws")]
    public async Task Scoping_NarrowScope_SecondSaveAfterClearContext_Throws()
    {
        // This is the exact pattern that caused the IAM login HTTP-500 bug.
        // SetContext covers Save1 but ClearContext fires before Save2 — Save2 throws.
        using var scope = new EdgeScope(tenantId: null);

        // SAVE 1 — context alive (correct)
        scope.UserSvc.SetContext(TenantX, "user@system");
        try
        {
            var entity1 = EntityCodeConfig.Create(string.Empty, "Save1");
            scope.Db.AuditableEntities.Add(entity1);
            await scope.Db.SaveChangesAsync();
            entity1.TenantId.Should().Be(TenantX, "Save1 inside SetContext scope must work");
        }
        finally
        {
            scope.UserSvc.ClearContext(); // ← too early — context killed here
        }

        // SAVE 2 — context already cleared (the bug pattern)
        var entity2 = EntityCodeConfig.Create(string.Empty, "Save2");
        scope.Db.AuditableEntities.Add(entity2);

        var act = async () => await scope.Db.SaveChangesAsync();
        await act.Should().ThrowAsync<InvalidOperationException>(
            "a save AFTER ClearContext was called must throw — " +
            "this is the exact LoginFeature bug pattern: narrow try/finally clears context " +
            "before a subsequent save that also needs TenantId");
    }

    [Fact(DisplayName = "SCOPING: SetContext scoped wide enough — both saves succeed")]
    public async Task Scoping_WideScope_BothSavesSucceed()
    {
        // The FIXED pattern: SetContext covers ALL saves, ClearContext only after all are done.
        // This mirrors the corrected LoginFeature code.
        using var scope = new EdgeScope(tenantId: null);

        scope.UserSvc.SetContext(TenantX, "user@system");
        try
        {
            // Save 1 (e.g. UpdateAsync — refresh token on UserIdentity)
            var entity1 = EntityCodeConfig.Create(string.Empty, "Save1");
            scope.Db.AuditableEntities.Add(entity1);
            await scope.Db.SaveChangesAsync();

            // Save 2 (e.g. CreateSessionAsync — UserSession AuditableEntity)
            var entity2 = EntityCodeConfig.Create(string.Empty, "Save2");
            scope.Db.AuditableEntities.Add(entity2);
            await scope.Db.SaveChangesAsync();
        }
        finally
        {
            scope.UserSvc.ClearContext(); // ← correct: cleared AFTER all saves
        }

        var all = await scope.Db.AuditableEntities.IgnoreQueryFilters().ToListAsync();
        all.Should().HaveCount(2).And.OnlyContain(e => e.TenantId == TenantX,
            "both saves must succeed when SetContext scope covers all DB operations");
    }

    [Fact(DisplayName = "SCOPING: Non-AuditableEntity save between SetContext and ClearContext does not break subsequent AuditableEntity save")]
    public async Task Scoping_NonAuditable_Between_StillAllowsAuditable()
    {
        // Variant of the login pattern: non-AuditableEntity save (UpdateAsync on IdentityUser)
        // followed by AuditableEntity save (CreateSessionAsync) — BOTH inside the SetContext scope.
        using var scope = new EdgeScope(tenantId: null);

        scope.UserSvc.SetContext(TenantX, "login@test.com");
        try
        {
            // Save 1: non-AuditableEntity (UserIdentity / IdentityUser) — no stamping needed
            scope.Db.IdentityUsers.Add(new FakeIdentityUser { TenantId = TenantX, UserName = "login@test.com" });
            await scope.Db.SaveChangesAsync();

            // Save 2: AuditableEntity (UserSession) — stamped from ambient context
            var session = EntityCodeConfig.Create(string.Empty, "UserSession");
            scope.Db.AuditableEntities.Add(session);
            await scope.Db.SaveChangesAsync();

            session.TenantId.Should().Be(TenantX,
                "AuditableEntity saved AFTER non-AuditableEntity but WITHIN the same SetContext scope must work");
        }
        finally
        {
            scope.UserSvc.ClearContext();
        }
    }
}
