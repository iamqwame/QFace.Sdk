using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using QFace.Sdk.Temporal.Interceptors;
using QimErp.Shared.Common.Diagnostics.IntegrityChecks;
using QimErp.Shared.Common.Services.Auth;
using QimErp.Shared.Common.Services.MultiTenancy;
using Xunit;

namespace QimErp.Shared.Common.Tests.Diagnostics.IntegrityChecks;

public sealed class IntegrityCheckRunnerBackgroundServiceTests
{
    private sealed class CleanCheck(string name) : IIntegrityCheck
    {
        public string Name { get; } = name;
        public int CallCount { get; private set; }

        public Task<IntegrityCheckResult> RunAsync(CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(IntegrityCheckResult.Clean(Name));
        }
    }

    private sealed class FindingsCheck(string name, long count) : IIntegrityCheck
    {
        public string Name { get; } = name;

        public Task<IntegrityCheckResult> RunAsync(CancellationToken cancellationToken) =>
            Task.FromResult(new IntegrityCheckResult(Name, count, ["sample-1"]));
    }

    private sealed class ThrowingCheck(string name) : IIntegrityCheck
    {
        public string Name { get; } = name;

        public Task<IntegrityCheckResult> RunAsync(CancellationToken cancellationToken) =>
            throw new InvalidOperationException("boom");
    }

    private sealed class TenantObservation
    {
        public string? TenantIdDuringSecondCheck { get; set; }
    }

    private sealed class TenantStompingCheck(ITenantScopeSetter tenantScopeSetter) : IIntegrityCheck
    {
        public string Name => "check.stomp";

        public Task<IntegrityCheckResult> RunAsync(CancellationToken cancellationToken)
        {
            tenantScopeSetter.SetTenantScope("tenant-from-check-a");
            return Task.FromResult(IntegrityCheckResult.Clean(Name));
        }
    }

    private sealed class TenantObservingCheck(ITenantContext tenantContext, TenantObservation observation) : IIntegrityCheck
    {
        public string Name => "check.observe";

        public Task<IntegrityCheckResult> RunAsync(CancellationToken cancellationToken)
        {
            observation.TenantIdDuringSecondCheck = tenantContext.TenantId;
            return Task.FromResult(IntegrityCheckResult.Clean(Name));
        }
    }

    private sealed class CountingScopeFactory(IServiceScopeFactory inner) : IServiceScopeFactory
    {
        public int CreateScopeCallCount { get; private set; }

        public IServiceScope CreateScope()
        {
            CreateScopeCallCount++;
            return inner.CreateScope();
        }
    }

    private static IntegrityCheckRunnerBackgroundService CreateRunner(IServiceProvider provider) =>
        new(provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<IntegrityCheckRunnerBackgroundService>.Instance,
            Microsoft.Extensions.Options.Options.Create(new IntegrityCheckRunnerOptions()));

    [Fact(DisplayName = "No registered checks => RunAllChecksAsync returns an empty list without throwing")]
    public async Task NoRegisteredChecks_ReturnsEmpty()
    {
        var provider = new ServiceCollection().AddLogging().BuildServiceProvider();
        var runner = CreateRunner(provider);

        var results = await runner.RunAllChecksAsync(CancellationToken.None);

        results.Should().BeEmpty();
    }

    [Fact(DisplayName = "Every registered IIntegrityCheck is run exactly once per sweep")]
    public async Task RunsEveryRegisteredCheck_Once()
    {
        var checkA = new CleanCheck("check.a");
        var checkB = new CleanCheck("check.b");

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IIntegrityCheck>(checkA);
        services.AddSingleton<IIntegrityCheck>(checkB);
        var provider = services.BuildServiceProvider();

        var runner = CreateRunner(provider);
        var results = await runner.RunAllChecksAsync(CancellationToken.None);

        checkA.CallCount.Should().Be(1);
        checkB.CallCount.Should().Be(1);
        results.Should().HaveCount(2);
        results.Select(r => r.CheckName).Should().BeEquivalentTo(["check.a", "check.b"]);
    }

    [Fact(DisplayName = "A check with findings is included in the returned results")]
    public async Task CheckWithFindings_IsIncludedInResults()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IIntegrityCheck>(new FindingsCheck("check.orphans", 3));
        var provider = services.BuildServiceProvider();

        var runner = CreateRunner(provider);
        var results = await runner.RunAllChecksAsync(CancellationToken.None);

        results.Should().ContainSingle();
        results[0].HasFindings.Should().BeTrue();
        results[0].Count.Should().Be(3);
        results[0].SampleIds.Should().BeEquivalentTo(["sample-1"]);
    }

    [Fact(DisplayName = "A check that throws is skipped without crashing the sweep, and other checks still run")]
    public async Task ThrowingCheck_IsSkipped_OtherChecksStillRun()
    {
        var okCheck = new CleanCheck("check.ok");

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IIntegrityCheck>(new ThrowingCheck("check.broken"));
        services.AddSingleton<IIntegrityCheck>(okCheck);
        var provider = services.BuildServiceProvider();

        var runner = CreateRunner(provider);
        var act = async () => await runner.RunAllChecksAsync(CancellationToken.None);
        var results = await act.Should().NotThrowAsync();

        okCheck.CallCount.Should().Be(1);
        results.Subject.Should().ContainSingle(r => r.CheckName == "check.ok");
    }

    [Fact(DisplayName = "RunAllChecksAsync runs each scoped check in its own DI scope")]
    public async Task RunAllChecksAsync_ResolvesEachCheckInItsOwnScope()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<IIntegrityCheck>(_ => new CleanCheck("check.scoped-a"));
        services.AddScoped<IIntegrityCheck>(_ => new CleanCheck("check.scoped-b"));
        var root = services.BuildServiceProvider();
        var scopeFactory = new CountingScopeFactory(root.GetRequiredService<IServiceScopeFactory>());

        var runner = new IntegrityCheckRunnerBackgroundService(
            scopeFactory,
            NullLogger<IntegrityCheckRunnerBackgroundService>.Instance,
            Microsoft.Extensions.Options.Options.Create(new IntegrityCheckRunnerOptions()));

        var before = scopeFactory.CreateScopeCallCount;
        var results = await runner.RunAllChecksAsync(CancellationToken.None);
        var scopesCreated = scopeFactory.CreateScopeCallCount - before;

        results.Should().HaveCount(2);
        scopesCreated.Should().Be(1 + 2, "one scope to count registrations plus one scope per check");
    }

    [Fact(DisplayName = "Tenant scope set by check A does not leak into check B")]
    public async Task TenantScopeFromCheckA_DoesNotLeakIntoCheckB()
    {
        var observation = new TenantObservation();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantContext, TenantContext>();
        services.AddScoped<ITenantScopeSetter>(sp => (ITenantScopeSetter)sp.GetRequiredService<ITenantContext>());
        services.AddScoped<ICurrentUserService, UserContextService>();
        services.AddSingleton(observation);
        services.AddScoped<IIntegrityCheck, TenantStompingCheck>();
        services.AddScoped<IIntegrityCheck, TenantObservingCheck>();
        var provider = services.BuildServiceProvider();

        var runner = CreateRunner(provider);
        await runner.RunAllChecksAsync(CancellationToken.None);

        observation.TenantIdDuringSecondCheck.Should().BeNull(
            "check B must not see tenant scope left behind by check A");

        using var verifyScope = provider.CreateScope();
        verifyScope.ServiceProvider.GetRequiredService<ITenantContext>().TenantId.Should().BeNull(
            "tenant scope must be cleared after each check, not leaked to the caller");
    }
}
