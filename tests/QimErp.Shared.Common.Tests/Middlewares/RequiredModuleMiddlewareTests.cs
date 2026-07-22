using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using QimErp.Shared.Common.Middlewares;
using QimErp.Shared.Common.Services.MultiTenancy;
using QimErp.Shared.Common.Services.TenantSetup;
using QimErp.Shared.Common.TenantSetup;
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
        await middleware.InvokeAsync(context, access, tenantContext);

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
        await middleware.InvokeAsync(context, access, tenantContext);

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
        await middleware.InvokeAsync(context, access, tenantContext);

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
        await middleware.InvokeAsync(context, access, tenantContext);

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        access.CallCount.Should().Be(0);
    }

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
