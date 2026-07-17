using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.Extensions;
using QimErp.Shared.Common.Interceptors;
using QimErp.Shared.Common.Options;
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

        // Assert
        interceptor.Should().NotBeNull();
    }

    [Fact]
    public void AddDbContextWithOutbox_WithConfiguration_ResolvesInterceptor()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContextWithOutbox<TestApplicationDbContext>(TestConnectionString, configuration);

        // Act
        using var scope = services.BuildServiceProvider().CreateScope();
        var interceptor = scope.ServiceProvider.GetService<AuditEntitySaveChangesInterceptor>();

        // Assert
        interceptor.Should().NotBeNull();
    }

    [Fact]
    public void AddDbContextWithOutboxConsumer_WithoutConfiguration_ResolvesConsumerAndInterceptor()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContextWithOutboxConsumer<TestApplicationDbContext>(TestConnectionString);

        // Act
        var provider = services.BuildServiceProvider();
        var consumer = provider.GetService<ConsumerUserContextService>();
        using var scope = provider.CreateScope();
        var interceptor = scope.ServiceProvider.GetService<AuditEntitySaveChangesInterceptor>();

        // Assert
        consumer.Should().NotBeNull();
        interceptor.Should().NotBeNull();
    }
}
