using QimErp.Shared.Common.Notifications.Models;

namespace QimErp.Shared.Common.Notifications.Providers;

public sealed class WebhookNotificationProvider(
    IHttpClientFactory httpClientFactory,
    ILogger<WebhookNotificationProvider> logger)
    : WebhookBasedNotificationProvider(httpClientFactory, logger)
{
    public override NotificationProviderId ProviderId => NotificationProviderId.Webhook;

    protected override object BuildWebhookBody(AlertPayload payload) =>
        new
        {
            source = "qimerp-workflow",
            payload.Title,
            payload.Body,
            payload.ActionUrl,
            payload.Severity,
            payload.Metadata,
            sentAt = DateTimeOffset.UtcNow,
        };
}
