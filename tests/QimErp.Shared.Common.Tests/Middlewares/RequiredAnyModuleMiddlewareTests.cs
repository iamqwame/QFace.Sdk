using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using QimErp.Shared.Common.Middlewares;
using QimErp.Shared.Common.Services.MultiTenancy;
using QimErp.Shared.Common.Services.TenantSetup;
using QimErp.Shared.Common.TenantSetup;
using Xunit;

namespace QimErp.Shared.Common.Tests.Middlewares;

public class RequiredAnyModuleMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_Allows_WhenTheSecondModuleIsInstalled()
    {
        var access = new FakeModuleAccess(ModuleKeys.POS);

        var context = await InvokeAsync(access, ModuleKeys.Inventory, ModuleKeys.POS);

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task InvokeAsync_Allows_WhenTheFirstModuleIsInstalled()
    {
        var access = new FakeModuleAccess(ModuleKeys.Inventory);

        var context = await InvokeAsync(access, ModuleKeys.Inventory, ModuleKeys.POS);

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        access.QueriedModuleKeys.Should().Equal(ModuleKeys.Inventory);
    }

    [Fact]
    public async Task InvokeAsync_Returns403_WhenNoModuleIsInstalled()
    {
        var access = new FakeModuleAccess(ModuleKeys.Payroll);

        var context = await InvokeAsync(access, ModuleKeys.Inventory, ModuleKeys.POS);

        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        access.QueriedModuleKeys.Should().Equal(ModuleKeys.Inventory, ModuleKeys.POS);
    }

    private static async Task<DefaultHttpContext> InvokeAsync(
        FakeModuleAccess access,
        params string[] moduleKeys)
    {
        var context = new DefaultHttpContext
        {
            Request = { Path = "/api/inventory/warehouses" },
            Response = { Body = new MemoryStream() },
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.Name, "tester")],
                authenticationType: "Test")),
        };

        var tenantContext = new TenantContext();
        tenantContext.SetTenant("tenant-1");

        var middleware = new RequiredModuleMiddleware(_ => Task.CompletedTask, moduleKeys);
        await middleware.InvokeAsync(
            context,
            access,
            tenantContext,
            new HostingEnvironment { EnvironmentName = Environments.Production });

        return context;
    }

    private sealed class FakeModuleAccess(params string[] installedModuleKeys) : ITenantModuleAccessService
    {
        public List<string> QueriedModuleKeys { get; } = [];

        public Task<bool> IsModuleEnabledAsync(
            string? tenantId,
            string moduleKey,
            CancellationToken cancellationToken = default)
        {
            QueriedModuleKeys.Add(moduleKey);
            return Task.FromResult(installedModuleKeys.Contains(moduleKey, StringComparer.OrdinalIgnoreCase));
        }

        public Task<IReadOnlyList<string>?> GetInstalledModuleKeysAsync(
            string? tenantId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<string>?>(installedModuleKeys);
    }
}
