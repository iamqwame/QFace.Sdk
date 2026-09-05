using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using MsOptions = Microsoft.Extensions.Options.Options;
using QimErp.Shared.Common.Authorization;
using QimErp.Shared.Common.Services.Auth;
using Xunit;

namespace QimErp.Shared.Common.Tests.Authorization;

public sealed class PermissionAuthorizationTests
{
    private const string CourseView = "learning.course.view";
    private const string CourseManage = "learning.course.manage";

    private static IConfiguration Configuration(bool? enforce) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(enforce is null
                ? []
                : new Dictionary<string, string?> { ["Security:EnforcePermissions"] = enforce.Value.ToString() })
            .Build();

    private static ICurrentUserService UserWith(params string[] permissions)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "user-001") };
        claims.AddRange(permissions.Select(p => new Claim("permissions", p)));

        var accessor = new StubHttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
            }
        };

        return new UserContextService(accessor, NullLogger<UserContextService>.Instance);
    }

    private static AuthorizationHandlerContext ContextFor(PermissionRequirement requirement) =>
        new([requirement], new ClaimsPrincipal(new ClaimsIdentity([], "TestAuth")), resource: null);

    [Fact(DisplayName = "Caller holding the permission succeeds while enforcement is on")]
    public async Task Caller_with_permission_succeeds()
    {
        var handler = new PermissionAuthorizationHandler(
            UserWith(CourseView), Configuration(enforce: true), NullLogger<PermissionAuthorizationHandler>.Instance);
        var context = ContextFor(new PermissionRequirement(CourseView));

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact(DisplayName = "Caller lacking the permission fails while enforcement is on")]
    public async Task Caller_without_permission_fails()
    {
        var handler = new PermissionAuthorizationHandler(
            UserWith(CourseView), Configuration(enforce: true), NullLogger<PermissionAuthorizationHandler>.Instance);
        var context = ContextFor(new PermissionRequirement(CourseManage));

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact(DisplayName = "Enforcement disabled succeeds and logs a warning naming the missing permission")]
    public async Task Enforcement_disabled_succeeds_with_warning()
    {
        var logger = new CapturingLogger<PermissionAuthorizationHandler>();
        var handler = new PermissionAuthorizationHandler(UserWith(), Configuration(enforce: false), logger);
        var context = ContextFor(new PermissionRequirement(CourseManage));

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
        logger.Warnings.Should().ContainSingle().Which.Should().Contain(CourseManage);
    }

    [Fact(DisplayName = "Enforcement defaults to disabled when the setting is absent")]
    public async Task Enforcement_defaults_to_disabled()
    {
        var handler = new PermissionAuthorizationHandler(
            UserWith(), Configuration(enforce: null), NullLogger<PermissionAuthorizationHandler>.Instance);
        var context = ContextFor(new PermissionRequirement(CourseManage));

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact(DisplayName = "Multiple codes are any-of, not all-of")]
    public async Task Multiple_codes_are_any_of()
    {
        var handler = new PermissionAuthorizationHandler(
            UserWith(CourseManage), Configuration(enforce: true), NullLogger<PermissionAuthorizationHandler>.Instance);
        var context = ContextFor(new PermissionRequirement($"{CourseView}|{CourseManage}"));

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact(DisplayName = "Policy provider materialises perm: policies on demand")]
    public async Task Provider_materialises_permission_policy()
    {
        var provider = new PermissionPolicyProvider(MsOptions.Create(new AuthorizationOptions()));

        var policy = await provider.GetPolicyAsync(PermissionPolicyProvider.PolicyNameFor([CourseView, CourseManage]));

        policy.Should().NotBeNull();
        policy!.Requirements.OfType<PermissionRequirement>().Should().ContainSingle()
            .Which.Codes.Should().BeEquivalentTo(CourseView, CourseManage);
    }

    [Fact(DisplayName = "Policy provider defers to the default provider for non-permission policy names")]
    public async Task Provider_defers_to_default_provider()
    {
        var options = new AuthorizationOptions();
        options.AddPolicy("ExistingPolicy", builder => builder.RequireRole("Admin"));
        var provider = new PermissionPolicyProvider(MsOptions.Create(options));

        var known = await provider.GetPolicyAsync("ExistingPolicy");
        var unknown = await provider.GetPolicyAsync("NeverRegistered");

        known.Should().NotBeNull();
        known!.Requirements.OfType<PermissionRequirement>().Should().BeEmpty();
        known.Requirements.OfType<RolesAuthorizationRequirement>().Should().ContainSingle();
        unknown.Should().BeNull();
    }

    [Fact(DisplayName = "GetPermissions returns empty rather than null when the claim is absent")]
    public void GetPermissions_returns_empty_when_claim_absent()
    {
        var permissions = UserWith().GetPermissions();

        permissions.Should().NotBeNull();
        permissions.Should().BeEmpty();
    }

    [Fact(DisplayName = "GetPermissions reads a comma separated permissions claim")]
    public void GetPermissions_reads_comma_separated_claim()
    {
        var accessor = new StubHttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim("permissions", $"{CourseView}, {CourseManage}")], "TestAuth"))
            }
        };
        var user = new UserContextService(accessor, NullLogger<UserContextService>.Instance);

        user.GetPermissions().Should().BeEquivalentTo(CourseView, CourseManage);
    }

    private sealed class StubHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; }
    }

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<string> Warnings { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.Warning)
                Warnings.Add(formatter(state, exception));
        }
    }
}
