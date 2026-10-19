using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.Extensions;
using QimErp.Shared.Common.Interceptors;
using QimErp.Shared.Common.Options;
using QimErp.Shared.Common.Outbox;
using QimErp.Shared.Common.Services.Auth;
using Xunit;

namespace QimErp.Shared.Common.Tests;

public class SharedServiceCollectionExtensionsTests
{
    private const string TestConnectionString = "Host=localhost;Database=test;Username=test;Password=test";

    [Fact]
    public void AddQimErpConfigurationWithDefaults_RegistersAllOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddQimErpConfigurationWithDefaults();
        var provider = services.BuildServiceProvider();

        // Assert
        var frontendSettings = provider.GetService<IOptions<FrontendSettings>>();
        frontendSettings.Should().NotBeNull();
        frontendSettings!.Value.BaseUrl.Should().Be(string.Empty);

        var systemOptions = provider.GetService<IOptions<SystemOptions>>();
        systemOptions.Should().NotBeNull();
        systemOptions!.Value.DefaultUserId.Should().Be("system");
    }

    [Fact]
    public void AddDbContextWithOutbox_WithoutConfiguration_ResolvesInterceptor()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContextWithOutbox<TestApplicationDbContext>(TestConnectionString);

        // Act
        using var scope = services.BuildServiceProvider().CreateScope();
        var interceptor = scope.ServiceProvider.GetService<AuditEntitySaveChangesInterceptor>();
        var outbox = scope.ServiceProvider.GetService<IOutbox>();

        // Assert
        interceptor.Should().NotBeNull();
        outbox.Should().NotBeNull("AddDbContextWithOutbox must register the durable DATA-09 outbox");
    }

    [Fact]
    public void AddDbContextWithOutbox_WithConfiguration_ResolvesInterceptor()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Outbox:RelayEnabled"] = "false",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContextWithOutbox<TestApplicationDbContext>(TestConnectionString, configuration);

        // Act
        using var scope = services.BuildServiceProvider().CreateScope();
        var interceptor = scope.ServiceProvider.GetService<AuditEntitySaveChangesInterceptor>();
        var outbox = scope.ServiceProvider.GetService<IOutbox>();

        // Assert
        interceptor.Should().NotBeNull();
        outbox.Should().NotBeNull();
    }

    [Fact]
    public void AddDbContextWithOutboxConsumer_WithoutConfiguration_ResolvesConsumerAndInterceptor()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        #pragma warning disable CS0618 // AddDbContextWithOutboxConsumer is obsolete; kept for compatibility tests
        services.AddDbContextWithOutboxConsumer<TestApplicationDbContext>(TestConnectionString);
#pragma warning restore CS0618

        // Act
        var provider = services.BuildServiceProvider();
        var consumer = provider.GetService<ConsumerUserContextService>();
        using var scope = provider.CreateScope();
        var interceptor = scope.ServiceProvider.GetService<AuditEntitySaveChangesInterceptor>();

        // Assert
        consumer.Should().NotBeNull();
        interceptor.Should().NotBeNull();
    }

    [Fact]
    public void AddDbContextWithOutbox_RegistersOutboxProcessorBackgroundService_ExactlyOnce()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContextWithOutbox<TestApplicationDbContext>(TestConnectionString);

        using var provider = services.BuildServiceProvider();
        var hostedServices = provider.GetServices<IHostedService>()
            .OfType<OutboxRelayHostedService>()
            .ToList();

        hostedServices.Should().ContainSingle(
            "AddDbContextWithOutbox must register the Akka outbox relay hosted service");
    }

    [Fact]
    public void AddDbContextWithOutbox_CalledTwice_StillRegistersOutboxRelayOnce()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContextWithOutbox<TestApplicationDbContext>(TestConnectionString);
        services.AddDbContextWithOutbox<TestApplicationDbContext>(TestConnectionString);

        using var provider = services.BuildServiceProvider();
        var hostedServices = provider.GetServices<IHostedService>()
            .OfType<OutboxRelayHostedService>()
            .ToList();

        hostedServices.Should().ContainSingle(
            "OutboxRelayHostedService must not duplicate when AddDbContextWithOutbox is called more than once");
    }

    [Fact]
    public void AddDbContextWithOutbox_WithoutConfiguration_UsesDefaultOutboxOptions()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContextWithOutbox<TestApplicationDbContext>(TestConnectionString);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<OutboxOptions>>().Value;

        options.BatchSize.Should().Be(100);
        options.RelayEnabled.Should().BeTrue();
        options.DefaultTransports.Should().Equal(OutboxTransports.InProcess);
    }

    [Fact]
    public void AddDbContextWithOutbox_WithConfiguration_BindsOutboxOptions()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{OutboxOptions.SectionName}:BatchSize"] = "25",
                [$"{OutboxOptions.SectionName}:RelayEnabled"] = "false"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContextWithOutbox<TestApplicationDbContext>(TestConnectionString, configuration);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<OutboxOptions>>().Value;

        options.BatchSize.Should().Be(25);
        options.RelayEnabled.Should().BeFalse();
    }
}
