using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using QFace.Sdk.Temporal.Interceptors;
using QimErp.Shared.Common.Services.Auth;
using Temporalio.Activities;
using Temporalio.Worker.Interceptors;
using Xunit;

namespace QimErp.Shared.Common.Tests.Temporal;

/// <summary>
/// Tests for <see cref="TenantContextActivityInterceptor"/>.
///
/// These tests verify the interceptor correctly seeds ICurrentUserService with the TenantId
/// from activity payloads. If ALL tests pass it is safe to remove the manual WithTenantId()
/// calls from activity code. Until then those calls remain as a safety net.
/// </summary>
public sealed class TenantContextActivityInterceptorTests
{
    private const string TenantA = "tenant-aaa-111";
    private const string TenantB = "tenant-bbb-222";

    // ── Test double ───────────────────────────────────────────────────────────

    private sealed class FakeTenantSetter : ICurrentUserService, ITenantContextSetter
    {
        public string? CapturedTenantId { get; private set; }
        public bool WasCleared { get; private set; }

        void ITenantContextSetter.SetTenantContext(string tenantId, string userEmail, string? userName, string? userId)
            => CapturedTenantId = tenantId;

        void ITenantContextSetter.ClearTenantContext()
        {
            WasCleared = true;
            CapturedTenantId = null;
        }

        void ICurrentUserService.SetContext(string tenantId, string userEmail, string? userName, string? userId)
            => CapturedTenantId = tenantId;

        void ICurrentUserService.ClearContext() { WasCleared = true; CapturedTenantId = null; }

        // ICurrentUserService stubs — not exercised here
        public string GetTenantId() => CapturedTenantId ?? string.Empty;
        public bool IsAuthenticated => false;
        public string GetCorrelationId() => string.Empty;
        public string GetUserId() => string.Empty;
        public string? GetRole() => null;
        public List<string> GetUserRoles() => [];
        public string? GetToken() => null;
        public IEnumerable<System.Security.Claims.Claim> GetClaims() => [];
        public string GetUserEmail() => string.Empty;
        public string GetUserName() => string.Empty;
        public string? GetDomainName() => null;
        public string? GetLanguage() => null;
        public string? GetTimeZone() => null;
        public string? GetCompanyName() => null;
        public string? GetEmployeeId() => null;
        public string? GetRankId() => null;
        public string? GetRankName() => null;
        public string? GetOrganizationalUnitId() => null;
        public string? GetOrganizationalUnitName() => null;
        public List<string> GetRoleIds() => [];
    }

    private record InputWithTenant(string TenantId, string RunId = "run-001");
    private record InputWithoutTenant(string Data = "no-tenant-here");

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static (TenantContextActivityInterceptor interceptor, FakeTenantSetter setter)
        BuildSut()
    {
        var setter = new FakeTenantSetter();
        var services = new ServiceCollection();
        services.AddSingleton<ITenantContextSetter>(setter);
        services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(NullLoggerFactory.Instance);
        var sp = services.BuildServiceProvider();
        var interceptor = new TenantContextActivityInterceptor(sp.GetRequiredService<IServiceScopeFactory>());
        return (interceptor, setter);
    }

    private static ExecuteActivityInput MakeInput(object payload)
    {
        // ExecuteActivityInput has no public constructor — build via reflection.
        var ctors = typeof(ExecuteActivityInput)
            .GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        var ctor = ctors.FirstOrDefault()
            ?? throw new InvalidOperationException("No ExecuteActivityInput constructor found.");

        var ps = ctor.GetParameters();
        var args = new object?[ps.Length];
        for (var i = 0; i < ps.Length; i++)
        {
            var p = ps[i];
            if (p.ParameterType == typeof(object[]))
                args[i] = new object[] { payload };
            else if (p.HasDefaultValue)
                args[i] = p.DefaultValue;
            else if (p.ParameterType.IsValueType)
                args[i] = Activator.CreateInstance(p.ParameterType);
            else
                args[i] = null;
        }

        return (ExecuteActivityInput)ctor.Invoke(args);
    }

    private static async Task<string?> RunAsync(
        TenantContextActivityInterceptor interceptor,
        FakeTenantSetter setter,
        object payload)
    {
        string? capturedAtCallTime = null;

        var terminal = new TerminalInterceptor(
            onExecute: () => capturedAtCallTime = setter.CapturedTenantId);

        var wrapped = interceptor.InterceptActivity(terminal);
        await wrapped.ExecuteActivityAsync(MakeInput(payload));
        return capturedAtCallTime;
    }

    private sealed class TerminalInterceptor : ActivityInboundInterceptor
    {
        private readonly Action _onExecute;
        public TerminalInterceptor(Action onExecute) : base(null!) => _onExecute = onExecute;

        public override Task<object?> ExecuteActivityAsync(ExecuteActivityInput input)
        {
            _onExecute();
            return Task.FromResult<object?>(null);
        }
    }

    private sealed class ThrowingInterceptor : ActivityInboundInterceptor
    {
        public ThrowingInterceptor() : base(null!) { }

        public override Task<object?> ExecuteActivityAsync(ExecuteActivityInput input)
            => throw new InvalidOperationException("Activity failed");
    }

    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "Seeds TenantId from input payload before activity executes")]
    public async Task Seeds_tenantId_before_execution()
    {
        var (interceptor, setter) = BuildSut();

        var tenantAtExecution = await RunAsync(interceptor, setter, new InputWithTenant(TenantA));

        tenantAtExecution.Should().Be(TenantA,
            "the interceptor must call SetTenantContext BEFORE the activity body runs");
    }

    [Fact(DisplayName = "Clears TenantId after activity completes successfully")]
    public async Task Clears_tenantId_after_successful_execution()
    {
        var (interceptor, setter) = BuildSut();

        await RunAsync(interceptor, setter, new InputWithTenant(TenantA));

        setter.WasCleared.Should().BeTrue("ClearTenantContext must be called in the finally block");
        setter.CapturedTenantId.Should().BeNull("TenantId must be null after activity completes");
    }

    [Fact(DisplayName = "Clears TenantId even when the activity throws")]
    public async Task Clears_tenantId_when_activity_throws()
    {
        var (interceptor, setter) = BuildSut();

        var wrapped = interceptor.InterceptActivity(new ThrowingInterceptor());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => wrapped.ExecuteActivityAsync(MakeInput(new InputWithTenant(TenantA))));

        setter.WasCleared.Should().BeTrue(
            "ClearTenantContext must run in finally even when the activity throws");
    }

    [Fact(DisplayName = "Activity without TenantId property does not throw (logs warning)")]
    public async Task Activity_without_tenantId_does_not_throw()
    {
        var (interceptor, setter) = BuildSut();

        var act = async () => await RunAsync(interceptor, setter, new InputWithoutTenant());

        await act.Should().NotThrowAsync(
            "an activity whose input has no TenantId property must not crash the worker");

        setter.CapturedTenantId.Should().BeNull(
            "no TenantId should be seeded when the input payload has no TenantId property");
    }

    [Fact(DisplayName = "CRITICAL: concurrent activities with different tenants do not bleed into each other")]
    public async Task Concurrent_activities_do_not_bleed_tenants()
    {
        // This is the most important test. If AsyncLocal isolation is broken,
        // TenantA's ID bleeds into TenantB's activity — cross-tenant data corruption.
        //
        // We run both activities concurrently with a small artificial delay inside the
        // activity body so their async continuations overlap. Each activity must see
        // ONLY its own TenantId.

        var setterA = new FakeTenantSetter();
        var setterB = new FakeTenantSetter();

        // Each "activity execution" gets its own scope/setter to simulate Temporal's
        // per-activity DI scope isolation.
        string? seenByA = null;
        string? seenByB = null;

        async Task SimulateActivity(string tenantId, FakeTenantSetter setter, int delayMs, Action<string?> capture)
        {
            var services = new ServiceCollection();
            services.AddSingleton<ITenantContextSetter>(setter);
            services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(NullLoggerFactory.Instance);
            var sp = services.BuildServiceProvider();
            var interceptor = new TenantContextActivityInterceptor(sp.GetRequiredService<IServiceScopeFactory>());

            var terminal = new TerminalInterceptor(onExecute: async () =>
            {
                await Task.Delay(delayMs); // overlap window
                capture(setter.CapturedTenantId);
            });

            // TerminalInterceptor.ExecuteActivityAsync is synchronous — use a wrapper
            var delayTerminal = new AsyncDelayInterceptor(delayMs, setter, capture);
            var wrapped = interceptor.InterceptActivity(delayTerminal);
            await wrapped.ExecuteActivityAsync(MakeInput(new InputWithTenant(tenantId)));
        }

        var taskA = SimulateActivity(TenantA, setterA, delayMs: 50, capture: v => seenByA = v);
        var taskB = SimulateActivity(TenantB, setterB, delayMs: 10, capture: v => seenByB = v);

        await Task.WhenAll(taskA, taskB);

        seenByA.Should().Be(TenantA, "activity A must see TenantA, not TenantB");
        seenByB.Should().Be(TenantB, "activity B must see TenantB, not TenantA");
    }

    private sealed class AsyncDelayInterceptor : ActivityInboundInterceptor
    {
        private readonly int _delayMs;
        private readonly FakeTenantSetter _setter;
        private readonly Action<string?> _capture;

        public AsyncDelayInterceptor(int delayMs, FakeTenantSetter setter, Action<string?> capture)
            : base(null!)
        {
            _delayMs = delayMs;
            _setter = setter;
            _capture = capture;
        }

        public override async Task<object?> ExecuteActivityAsync(ExecuteActivityInput input)
        {
            await Task.Delay(_delayMs);
            _capture(_setter.CapturedTenantId);
            return null;
        }
    }
}
