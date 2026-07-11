using QimErp.Shared.Common.Notifications.Models;

namespace QimErp.Shared.Common.Notifications.Providers;

public sealed class DiscordNotificationProvider(
    IHttpClientFactory httpClientFactory,
    ILogger<DiscordNotificationProvider> logger)
    : WebhookBasedNotificationProvider(httpClientFactory, logger)
{
    public override NotificationProviderId ProviderId => NotificationProviderId.Discord;

    protected override object BuildWebhookBody(AlertPayload payload) =>
        new { content = $"**{payload.Title}**\n{payload.Body}" };
}
