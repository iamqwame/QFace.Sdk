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
        frontendSettings!.Value.BaseUrl.Should().Be("https://app.qimerp.com");

        var systemOptions = provider.GetService<IOptions<SystemOptions>>();
        systemOptions.Should().NotBeNull();
        systemOptions!.Value.DefaultUserId.Should().Be("system");

        var rabbitMqOptions = provider.GetService<IOptions<RabbitMqOptions>>();
        rabbitMqOptions.Should().NotBeNull();
        rabbitMqOptions!.Value.WorkflowApprovalRequiredExchange.Should().Be(RabbitMqOptions.DefaultWorkflowApprovalRequiredExchange);
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
    public void AddDbContextWithOutbox_WithConfiguration_BindsOptions()
    {
        // Arrange
        const string customExchange = "custom.workflow.approval.exchange";
        var configData = new Dictionary<string, string?>
        {
            ["RabbitMq:WorkflowApprovalRequiredExchange"] = customExchange
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContextWithOutbox<TestApplicationDbContext>(TestConnectionString, configuration);

        // Act
        var provider = services.BuildServiceProvider();
        var rabbitMqOptions = provider.GetRequiredService<IOptions<RabbitMqOptions>>();

        // Assert
        rabbitMqOptions.Value.WorkflowApprovalRequiredExchange.Should().Be(customExchange);
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
