using QimErp.Shared.Common.Notifications.Models;

namespace QimErp.Shared.Common.Notifications.Providers;

public sealed class SlackNotificationProvider(
    IHttpClientFactory httpClientFactory,
    ILogger<SlackNotificationProvider> logger)
    : WebhookBasedNotificationProvider(httpClientFactory, logger)
{
    public override NotificationProviderId ProviderId => NotificationProviderId.Slack;

    protected override object BuildWebhookBody(AlertPayload payload) =>
        new { text = $"*{payload.Title}*\n{payload.Body}" };
}
