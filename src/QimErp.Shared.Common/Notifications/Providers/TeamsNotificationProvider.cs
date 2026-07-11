using QimErp.Shared.Common.Notifications.Models;

namespace QimErp.Shared.Common.Notifications.Providers;

public sealed class TeamsNotificationProvider(
    IHttpClientFactory httpClientFactory,
    ILogger<TeamsNotificationProvider> logger)
    : WebhookBasedNotificationProvider(httpClientFactory, logger)
{
    public override NotificationProviderId ProviderId => NotificationProviderId.MicrosoftTeams;

    protected override object BuildWebhookBody(AlertPayload payload) =>
        new
        {
            type = "message",
            attachments = new[]
            {
                new
                {
                    contentType = "application/vnd.microsoft.card.adaptive",
                    content = new
                    {
                        type = "AdaptiveCard",
                        version = "1.4",
                        body = new object[]
                        {
                            new { type = "TextBlock", text = payload.Title, weight = "Bolder", size = "Medium" },
                            new { type = "TextBlock", text = payload.Body, wrap = true },
                        },
                        actions = string.IsNullOrWhiteSpace(payload.ActionUrl)
                            ? Array.Empty<object>()
                            : new object[]
                            {
                                new { type = "Action.OpenUrl", title = "Open", url = payload.ActionUrl },
                            },
                    },
                },
            },
        };
}
