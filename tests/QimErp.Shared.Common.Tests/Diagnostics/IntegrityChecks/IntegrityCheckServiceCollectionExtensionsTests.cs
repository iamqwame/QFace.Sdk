using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using QimErp.Shared.Common.Diagnostics.IntegrityChecks;
using QimErp.Shared.Common.Extensions;
using Xunit;

namespace QimErp.Shared.Common.Tests.Diagnostics.IntegrityChecks;

public class IntegrityCheckServiceCollectionExtensionsTests
{
    private const string TestConnectionString = "Host=localhost;Database=test;Username=test;Password=test";

    private sealed class CheckA : IIntegrityCheck
    {
        public string Name => "check.a";
        public Task<IntegrityCheckResult> RunAsync(CancellationToken cancellationToken) =>
            Task.FromResult(IntegrityCheckResult.Clean(Name));
    }

    private sealed class CheckB : IIntegrityCheck
    {
        public string Name => "check.b";
        public Task<IntegrityCheckResult> RunAsync(CancellationToken cancellationToken) =>
            Task.FromResult(IntegrityCheckResult.Clean(Name));
    }

    [Fact]
    public void AddIntegrityCheck_RegistersAsIIntegrityCheck()
    {
        var services = new ServiceCollection();
        services.AddIntegrityCheck<CheckA>();

        using var provider = services.BuildServiceProvider();
        var checks = provider.GetServices<IIntegrityCheck>().ToList();

        checks.Should().ContainSingle(c => c is CheckA);
    }

    [Fact]
    public void AddIntegrityCheck_CalledTwiceForSameType_StillRegistersOnce()
    {
        var services = new ServiceCollection();
        services.AddIntegrityCheck<CheckA>();
        services.AddIntegrityCheck<CheckA>();

        using var provider = services.BuildServiceProvider();
        var checks = provider.GetServices<IIntegrityCheck>().ToList();

        checks.Should().ContainSingle(
            "TryAddEnumerable must not duplicate the same check implementation when registered twice");
    }

    [Fact]
    public void AddIntegrityCheck_CalledForDifferentTypes_RegistersBoth()
    {
        var services = new ServiceCollection();
        services.AddIntegrityCheck<CheckA>();
        services.AddIntegrityCheck<CheckB>();

        using var provider = services.BuildServiceProvider();
        var checks = provider.GetServices<IIntegrityCheck>().ToList();

        checks.Should().HaveCount(2);
        checks.Select(c => c.Name).Should().BeEquivalentTo(["check.a", "check.b"]);
    }

    [Fact]
    public void AddIntegrityCheckRunner_WithoutConfiguration_UsesDefaultOptions()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddIntegrityCheckRunner();

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<IntegrityCheckRunnerOptions>>().Value;

        options.PollingIntervalSeconds.Should().Be(86_400);
    }

    [Fact]
    public void AddIntegrityCheckRunner_WithConfiguration_BindsOptions()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{IntegrityCheckRunnerOptions.SectionName}:PollingIntervalSeconds"] = "3600"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddIntegrityCheckRunner(configuration);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<IntegrityCheckRunnerOptions>>().Value;

        options.PollingIntervalSeconds.Should().Be(3600);
    }

    [Fact]
    public void AddIntegrityCheckRunner_RegistersBackgroundService_ExactlyOnce()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddIntegrityCheckRunner();

        using var provider = services.BuildServiceProvider();
        var hostedServices = provider.GetServices<IHostedService>()
            .OfType<IntegrityCheckRunnerBackgroundService>()
            .ToList();

        hostedServices.Should().ContainSingle();
    }

    [Fact]
    public void AddIntegrityCheckRunner_CalledTwice_StillRegistersBackgroundServiceOnce()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddIntegrityCheckRunner();
        services.AddIntegrityCheckRunner();

        using var provider = services.BuildServiceProvider();
        var hostedServices = provider.GetServices<IHostedService>()
            .OfType<IntegrityCheckRunnerBackgroundService>()
            .ToList();

        hostedServices.Should().ContainSingle(
            "TryAddEnumerable must not duplicate the runner when AddIntegrityCheckRunner is called more than once");
    }

    [Fact(DisplayName = "AddDbContextWithOutbox does not wire the integrity check runner — it is opt-in only")]
    public void AddDbContextWithOutbox_DoesNotRegisterIntegrityCheckRunner()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContextWithOutbox<TestApplicationDbContext>(TestConnectionString);

        using var provider = services.BuildServiceProvider();
        var hostedServices = provider.GetServices<IHostedService>()
            .OfType<IntegrityCheckRunnerBackgroundService>()
            .ToList();

        hostedServices.Should().BeEmpty(
            "DATA-06 integrity checks are opt-in via AddIntegrityCheckRunner, not implied by AddDbContextWithOutbox");
    }

    [Fact(DisplayName = "AddDbContextWithOutbox does not register any IIntegrityCheck")]
    public void AddDbContextWithOutbox_DoesNotRegisterAnyIntegrityCheck()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContextWithOutbox<TestApplicationDbContext>(TestConnectionString);

        using var provider = services.BuildServiceProvider();
        var checks = provider.GetServices<IIntegrityCheck>().ToList();

        checks.Should().BeEmpty();
    }
}
