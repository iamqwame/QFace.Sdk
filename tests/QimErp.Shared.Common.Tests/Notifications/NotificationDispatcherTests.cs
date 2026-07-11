using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using QimErp.Shared.Common.Notifications;
using QimErp.Shared.Common.Notifications.Models;
using QimErp.Shared.Common.Notifications.Providers;
using QimErp.Shared.Common.Services.TenantSetup;
using QimErp.Shared.Common.TenantSetup;
using Xunit;

namespace QimErp.Shared.Common.Tests.Notifications;

public class NotificationDispatcherTests
{
    [Fact]
    public async Task DispatchAlertAsync_OnlyDispatchesInstalledAndEnabledProviders()
    {
        var tenantId = Guid.NewGuid().ToString();
        var config = new Mock<ITenantNotificationChannelConfigService>();
        config.Setup(s => s.GetEnabledProvidersAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new NotificationProviderSettings
                {
                    Id = NotificationProviderId.Slack,
                    Enabled = true,
                    Config = new Dictionary<string, string>(),
                },
                new NotificationProviderSettings
                {
                    Id = NotificationProviderId.Sms,
                    Enabled = true,
                    Config = new Dictionary<string, string>(),
                },
            ]);

        var pluginAccess = new Mock<ITenantPluginAccessService>();
        pluginAccess.Setup(p => p.IsPluginEnabledAsync(tenantId, PluginKeys.ChatNotify, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        pluginAccess.Setup(p => p.IsPluginEnabledAsync(tenantId, PluginKeys.SmsNotify, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var slack = new SlackNotificationProvider(new StubHttpClientFactory(), NullLogger<SlackNotificationProvider>.Instance);
        var sms = new SmsNotificationProvider(new StubHttpClientFactory(), NullLogger<SmsNotificationProvider>.Instance);

        var dispatcher = new NotificationDispatcher(
            [slack, sms],
            config.Object,
            pluginAccess.Object,
            NullLogger<NotificationDispatcher>.Instance);

        var results = await dispatcher.DispatchAlertAsync(
            tenantId,
            new AlertPayload { Title = "Test", Body = "Body" });

        results.Should().ContainSingle();
        results[0].ProviderId.Should().Be(NotificationProviderId.Slack);
        results[0].Success.Should().BeTrue();
    }

    [Fact]
    public async Task DispatchTestAlertAsync_ReturnsFailureWhenPluginNotInstalled()
    {
        var tenantId = Guid.NewGuid().ToString();
        var config = new Mock<ITenantNotificationChannelConfigService>();
        config.Setup(s => s.GetPluginSettingsAsync(PluginKeys.ChatNotify, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PluginNotificationSettings
            {
                Providers =
                [
                    new NotificationProviderSettings
                    {
                        Id = NotificationProviderId.Slack,
                        Enabled = true,
                    },
                ],
            });

        var pluginAccess = new Mock<ITenantPluginAccessService>();
        pluginAccess.Setup(p => p.IsPluginEnabledAsync(tenantId, PluginKeys.ChatNotify, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var dispatcher = new NotificationDispatcher(
            [new SlackNotificationProvider(new StubHttpClientFactory(), NullLogger<SlackNotificationProvider>.Instance)],
            config.Object,
            pluginAccess.Object,
            NullLogger<NotificationDispatcher>.Instance);

        var result = await dispatcher.DispatchTestAlertAsync(
            tenantId,
            NotificationProviderId.Slack,
            new AlertPayload { Title = "Test", Body = "Body" });

        result.Success.Should().BeFalse();
        result.Error.Should().Contain(PluginKeys.ChatNotify);
    }

    private sealed class StubHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new();
    }
}
