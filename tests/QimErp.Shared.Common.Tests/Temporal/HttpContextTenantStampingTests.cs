using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
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
/// Integration tests for the HTTP → EF Core TenantId stamping chain.
///
/// Complements <see cref="BackgroundServiceTenantStampingTests"/> which tests the
/// background-service path. Together they verify the full picture:
///
///   HTTP path:  JWT claim TenantId → IHttpContextAccessor → UserContextService.GetTenantId()
///                   → AuditEntitySaveChangesInterceptor → entity stamped
///
///   Background: SetContext(tenantId) → AsyncLocal → UserContextService.GetTenantId()
///                   → AuditEntitySaveChangesInterceptor → entity stamped
///
/// Key invariant: HTTP path MUST work correctly and NOT be broken by the
/// background-service SetContext changes (AsyncLocal is checked FIRST — it must
/// not contaminate the HTTP path when it is empty).
/// </summary>
public sealed class HttpContextTenantStampingTests
{
    private const string HttpTenantId  = "019e31ec-http-1111-0000-000000000001";
    private const string BgTenantId    = "019e31ec-bg---2222-0000-000000000002";

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Builds a fake HttpContext with the given TenantId in the claims principal.</summary>
    private static DefaultHttpContext MakeHttpContext(string tenantId, string? userId = "http-user-001")
    {
        var claims = new List<Claim>
        {
            new("TenantId",                          tenantId),
            new(ClaimTypes.NameIdentifier,           userId ?? "http-user"),
            new(ClaimTypes.Email,                    "http-user@techlabs.com"),
            new(ClaimTypes.Name,                     "HTTP User"),
        };
        var identity  = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        return new DefaultHttpContext { User = principal };
    }

    private sealed class FakeHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; }
    }

    private sealed class TestScope : IDisposable
    {
        private readonly ServiceProvider _root;
        private readonly IServiceScope   _scope;

        public MinimalTestDbContext    Db       { get; }
        public UserContextService      UserSvc  { get; }
        public FakeHttpContextAccessor Accessor { get; }

        public TestScope(HttpContext? httpCtx = null, string? dbName = null)
        {
            Accessor = new FakeHttpContextAccessor { HttpContext = httpCtx };

            var services = new ServiceCollection();
            services.AddSingleton<IHttpContextAccessor>(Accessor);
            services.AddScoped<UserContextService>();
            services.AddScoped<ICurrentUserService>(sp => sp.GetRequiredService<UserContextService>());
            services.AddScoped<ITenantContext, TenantContext>();
            services.AddLogging();
            services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(NullLoggerFactory.Instance);
            services.AddScoped<AuditEntitySaveChangesInterceptor>();

            _root  = services.BuildServiceProvider();
            _scope = _root.CreateScope();
            var sp = _scope.ServiceProvider;

            UserSvc = sp.GetRequiredService<UserContextService>();

            var interceptor = sp.GetRequiredService<AuditEntitySaveChangesInterceptor>();
            var opts = new DbContextOptionsBuilder<MinimalTestDbContext>()
                .UseInMemoryDatabase(dbName ?? $"httptest-{Guid.NewGuid()}")
                .AddInterceptors(interceptor)
                .Options;

            Db = new MinimalTestDbContext(opts, sp.GetRequiredService<ITenantContext>());
        }

        public void Dispose() { _scope.Dispose(); _root.Dispose(); }
    }

    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "HTTP path: TenantId stamped from JWT claim when no AsyncLocal context is set")]
    public async Task TenantId_stamped_from_jwt_claim_on_http_request()
    {
        using var scope = new TestScope(MakeHttpContext(HttpTenantId));

        // No SetContext — TenantId comes purely from the HTTP claim
        var entity = EntityCodeConfig.Create(string.Empty, "HttpPathEntity");
        scope.Db.EntityCodeConfigs.Add(entity);

        await scope.Db.SaveChangesAsync();

        entity.TenantId.Should().Be(HttpTenantId,
            "AuditEntitySaveChangesInterceptor must read TenantId from the JWT claim " +
            "via ICurrentUserService.GetTenantId() when no AsyncLocal context is set");
    }

    [Fact(DisplayName = "HTTP path: Throws when no JWT TenantId claim and no SetContext (no tenant at all)")]
    public async Task Throws_when_http_request_has_no_tenant_claim()
    {
        // Empty HttpContext — authenticated user but no TenantId claim
        var httpCtx = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Email, "anonymous@test.com") }, "TestAuth"))
        };

        using var scope = new TestScope(httpCtx);

        var entity = EntityCodeConfig.Create(string.Empty, "NoClaimEntity");
        scope.Db.EntityCodeConfigs.Add(entity);

        var act = async () => await scope.Db.SaveChangesAsync();

        await act.Should().ThrowAsync<InvalidOperationException>(
            "an authenticated HTTP request with no TenantId claim must fail " +
            "rather than silently save a NULL-tenanted row");
    }

    [Fact(DisplayName = "HTTP path: AsyncLocal (background) takes precedence over JWT claim when both set")]
    public async Task AsyncLocal_takes_precedence_over_jwt_claim()
    {
        // HTTP context has HTTP tenant but background SetContext has a DIFFERENT tenant
        using var scope = new TestScope(MakeHttpContext(HttpTenantId));

        // Simulate: HTTP request handler kicks off a background job and calls SetContext
        // with a DIFFERENT tenantId (e.g. a system-level operation on another tenant)
        scope.UserSvc.SetContext(BgTenantId, "bg@system");

        var entity = EntityCodeConfig.Create(string.Empty, "PrecedenceEntity");
        scope.Db.EntityCodeConfigs.Add(entity);
        await scope.Db.SaveChangesAsync();

        entity.TenantId.Should().Be(BgTenantId,
            "AsyncLocal (set via SetContext) takes precedence over JWT claim — " +
            "this is by design so background jobs can operate on a specific tenant " +
            "even when the host request is for a different one");
    }

    [Fact(DisplayName = "HTTP path: JWT claim is restored after ClearContext (AsyncLocal cleared, HTTP claim still present)")]
    public async Task Jwt_claim_used_after_ClearContext()
    {
        // HTTP context with tenant
        using var scope = new TestScope(MakeHttpContext(HttpTenantId));

        // Set a background context, then clear it — HTTP claim should resume
        scope.UserSvc.SetContext(BgTenantId, "bg@system");
        scope.UserSvc.ClearContext();

        // Now GetTenantId() must fall back to the JWT claim
        var entity = EntityCodeConfig.Create(string.Empty, "FallbackToHttpEntity");
        scope.Db.EntityCodeConfigs.Add(entity);
        await scope.Db.SaveChangesAsync();

        entity.TenantId.Should().Be(HttpTenantId,
            "after ClearContext(), GetTenantId() must fall back to the JWT claim — " +
            "the HTTP path is restored as soon as AsyncLocal is cleared");
    }

    [Fact(DisplayName = "HTTP path: null HttpContext (no active request) with no SetContext throws")]
    public async Task Null_http_context_and_no_SetContext_throws()
    {
        // No HttpContext at all (e.g. a worker thread with no active request)
        using var scope = new TestScope(httpCtx: null);

        var entity = EntityCodeConfig.Create(string.Empty, "NullHttpEntity");
        scope.Db.EntityCodeConfigs.Add(entity);

        var act = async () => await scope.Db.SaveChangesAsync();

        await act.Should().ThrowAsync<InvalidOperationException>(
            "with no HTTP context and no SetContext call there is absolutely no tenant — " +
            "the interceptor must throw, not silently write a NULL-tenanted row");
    }
}
