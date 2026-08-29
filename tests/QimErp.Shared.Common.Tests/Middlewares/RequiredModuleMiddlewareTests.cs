using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using QimErp.Shared.Common.Constants;
using QimErp.Shared.Common.Contracts;
using QimErp.Shared.Common.Middlewares;
using QimErp.Shared.Common.Services.Cache;
using QimErp.Shared.Common.Services.MultiTenancy;
using QimErp.Shared.Common.Services.TenantSetup;
using QimErp.Shared.Common.TenantSetup;
using QimErp.Shared.Common.Tests.TenantSetup;
using Xunit;

namespace QimErp.Shared.Common.Tests.Middlewares;

public class RequiredModuleMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_AllowsHealthChecks_WithoutModuleLookup()
    {
        var access = new FakeTenantModuleAccessService(shouldAllow: false);
        var context = CreateHttpContext("/health", authenticated: true);
        var tenantContext = new TenantContext();
        tenantContext.SetTenant("tenant-1");

        var middleware = new RequiredModuleMiddleware(_ => Task.CompletedTask, ModuleKeys.Payroll);
        await middleware.InvokeAsync(context, access, tenantContext, CreateEnvironment("Production"));

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        access.CallCount.Should().Be(0);
    }

    [Fact]
    public async Task InvokeAsync_Returns403_WhenOptionalModuleNotInstalled()
    {
        var access = new FakeTenantModuleAccessService(shouldAllow: false);
        var context = CreateHttpContext("/api/payroll/runs", authenticated: true);
        var tenantContext = new TenantContext();
        tenantContext.SetTenant("tenant-1");

        var middleware = new RequiredModuleMiddleware(_ => Task.CompletedTask, ModuleKeys.Payroll);
        await middleware.InvokeAsync(context, access, tenantContext, CreateEnvironment("Production"));

        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        access.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task InvokeAsync_Returns403_WhenQimAiModuleNotInstalled()
    {
        var access = new FakeTenantModuleAccessService(shouldAllow: false);
        var context = CreateHttpContext("/api/platform-intelligence/collections/hr-resources/ask", authenticated: true);
        var tenantContext = new TenantContext();
        tenantContext.SetTenant("tenant-1");

        var middleware = new RequiredModuleMiddleware(_ => Task.CompletedTask, ModuleKeys.QimAI);
        await middleware.InvokeAsync(context, access, tenantContext, CreateEnvironment("Production"));

        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        access.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task InvokeAsync_SkipsCheck_ForUnauthenticatedRequests()
    {
        var access = new FakeTenantModuleAccessService(shouldAllow: false);
        var context = CreateHttpContext("/api/payroll/runs", authenticated: false);
        var tenantContext = new TenantContext();

        var middleware = new RequiredModuleMiddleware(_ => Task.CompletedTask, ModuleKeys.Payroll);
        await middleware.InvokeAsync(context, access, tenantContext, CreateEnvironment("Production"));

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        access.CallCount.Should().Be(0);
    }

    [Fact]
    public async Task InvokeAsync_SkipsCheck_InTestEnvironment()
    {
        var access = new FakeTenantModuleAccessService(shouldAllow: false);
        var context = CreateHttpContext("/api/payroll/runs", authenticated: true);
        var tenantContext = new TenantContext();
        tenantContext.SetTenant("tenant-1");

        var middleware = new RequiredModuleMiddleware(_ => Task.CompletedTask, ModuleKeys.Payroll);
        await middleware.InvokeAsync(context, access, tenantContext, CreateEnvironment("Test"));

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        access.CallCount.Should().Be(0);
    }

    [Fact]
    public async Task InvokeAsync_Returns403_WhenAuthenticatedRequestHasNoTenant()
    {
        var context = CreateHttpContext("/api/inventory/stock-issues", authenticated: true);
        var tenantContext = new TenantContext();
        tenantContext.SetTenant(null);

        var middleware = new RequiredModuleMiddleware(_ => Task.CompletedTask, ModuleKeys.Inventory);
        await middleware.InvokeAsync(
            context, CreateRealAccessService(), tenantContext, CreateEnvironment("Production"));

        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task InvokeAsync_Returns403_WhenAuthenticatedRequestHasEmptyTenant()
    {
        var context = CreateHttpContext("/api/inventory/stock-issues", authenticated: true);
        var tenantContext = new TenantContext();
        tenantContext.SetTenant("   ");

        var middleware = new RequiredModuleMiddleware(_ => Task.CompletedTask, ModuleKeys.Inventory);
        await middleware.InvokeAsync(
            context, CreateRealAccessService(), tenantContext, CreateEnvironment("Production"));

        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task InvokeAsync_Returns403_ForBaseModelModule_WhenTenantIsMissing()
    {
        var context = CreateHttpContext("/api/employees", authenticated: true);
        var tenantContext = new TenantContext();
        tenantContext.SetTenant(null);

        var middleware = new RequiredModuleMiddleware(_ => Task.CompletedTask, ModuleKeys.CoreHR);
        await middleware.InvokeAsync(
            context, CreateRealAccessService(), tenantContext, CreateEnvironment("Production"));

        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task InvokeAsync_Allows_WhenTenantValidAndModuleInstalled()
    {
        var access = CreateRealAccessService(await CreateCacheWithModulesAsync(ModuleKeys.Inventory));
        var context = CreateHttpContext("/api/inventory/stock-issues", authenticated: true);
        var tenantContext = new TenantContext();
        tenantContext.SetTenant(SnapshotTenantId);

        var nextCalled = false;
        var middleware = new RequiredModuleMiddleware(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            ModuleKeys.Inventory);
        await middleware.InvokeAsync(context, access, tenantContext, CreateEnvironment("Production"));

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_Returns403_WhenTenantValidButModuleNotInstalled()
    {
        var access = CreateRealAccessService(await CreateCacheWithModulesAsync(ModuleKeys.POS));
        var context = CreateHttpContext("/api/inventory/stock-issues", authenticated: true);
        var tenantContext = new TenantContext();
        tenantContext.SetTenant(SnapshotTenantId);

        var middleware = new RequiredModuleMiddleware(_ => Task.CompletedTask, ModuleKeys.Inventory);
        await middleware.InvokeAsync(context, access, tenantContext, CreateEnvironment("Production"));

        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Theory]
    [InlineData(ModuleKeys.CoreHR)]
    [InlineData(ModuleKeys.Leave)]
    public async Task InvokeAsync_AllowsBaseModelModule_ForValidTenant_WithoutSnapshot(string moduleKey)
    {
        var context = CreateHttpContext("/api/employees", authenticated: true);
        var tenantContext = new TenantContext();
        tenantContext.SetTenant(SnapshotTenantId);

        var middleware = new RequiredModuleMiddleware(_ => Task.CompletedTask, moduleKey);
        await middleware.InvokeAsync(
            context, CreateRealAccessService(), tenantContext, CreateEnvironment("Production"));

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Theory]
    [InlineData("/api/auth/login")]
    [InlineData("/api/auth/refresh-token")]
    [InlineData("/api/tenants/register")]
    public async Task InvokeAsync_AllowsAnonymousTenantlessRequest(string path)
    {
        var context = CreateHttpContext(path, authenticated: false);
        var tenantContext = new TenantContext();
        tenantContext.SetTenant(null);

        var middleware = new RequiredModuleMiddleware(_ => Task.CompletedTask, ModuleKeys.Inventory);
        await middleware.InvokeAsync(
            context, CreateRealAccessService(), tenantContext, CreateEnvironment("Production"));

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/alive")]
    [InlineData("/ready")]
    [InlineData("/swagger/index.html")]
    public async Task InvokeAsync_AllowsInfrastructurePaths_WhenTenantIsMissing(string path)
    {
        var context = CreateHttpContext(path, authenticated: true);
        var tenantContext = new TenantContext();
        tenantContext.SetTenant(null);

        var middleware = new RequiredModuleMiddleware(_ => Task.CompletedTask, ModuleKeys.Inventory);
        await middleware.InvokeAsync(
            context, CreateRealAccessService(), tenantContext, CreateEnvironment("Production"));

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task InvokeAsync_AllowsTestEnvironment_WhenTenantIsMissing()
    {
        var context = CreateHttpContext("/api/inventory/stock-issues", authenticated: true);
        var tenantContext = new TenantContext();
        tenantContext.SetTenant(null);

        var middleware = new RequiredModuleMiddleware(_ => Task.CompletedTask, ModuleKeys.Inventory);
        await middleware.InvokeAsync(
            context, CreateRealAccessService(), tenantContext, CreateEnvironment("Test"));

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    private const string SnapshotTenantId = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb";

    private static TenantModuleAccessService CreateRealAccessService(IDistributedCacheService? cache = null) =>
        new(cache ?? new InMemoryDistributedCacheService(), new FakeHttpContextAccessor());

    private static async Task<IDistributedCacheService> CreateCacheWithModulesAsync(params string[] moduleKeys)
    {
        var cache = new InMemoryDistributedCacheService();
        await cache.SetAsync(
            SharedCacheKeys.TenantModuleSnapshot(Guid.Parse(SnapshotTenantId)),
            new TenantModuleSnapshotEntry(1, moduleKeys),
            expiration: null);
        return cache;
    }

    private static IHostEnvironment CreateEnvironment(string environmentName) =>
        new HostingEnvironment { EnvironmentName = environmentName };

    private static DefaultHttpContext CreateHttpContext(string path, bool authenticated)
    {
        var context = new DefaultHttpContext
        {
            Request = { Path = path },
            Response = { Body = new MemoryStream() },
        };

        if (authenticated)
        {
            context.User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.Name, "tester")],
                authenticationType: "Test"));
        }

        return context;
    }

    private sealed class FakeTenantModuleAccessService(bool shouldAllow) : ITenantModuleAccessService
    {
        public int CallCount { get; private set; }

        public Task<bool> IsModuleEnabledAsync(
            string? tenantId,
            string moduleKey,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(shouldAllow);
        }

        public Task<IReadOnlyList<string>?> GetInstalledModuleKeysAsync(
            string? tenantId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<string>?>([]);
    }
}
