using System.Net.Http.Json;
using System.Text.Json;
using QimErp.Shared.Common.Notifications.Models;

namespace QimErp.Shared.Common.Notifications.Providers;

public abstract class WebhookBasedNotificationProvider(
    IHttpClientFactory httpClientFactory,
    ILogger logger) : INotificationChannelProvider
{
    public abstract NotificationProviderId ProviderId { get; }
    public NotificationChannelFamily Family => NotificationProviderMetadata.GetFamily(ProviderId);
    public string PluginKey => NotificationProviderMetadata.GetPluginKey(ProviderId);
    public int DispatchOrder => NotificationProviderMetadata.GetDispatchOrder(ProviderId);

    public virtual Task<MeetingResult?> CreateMeetingAsync(
        MeetingRequest request,
        IReadOnlyDictionary<string, string> config,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<MeetingResult?>(null);

    public async Task<SendResult> SendAlertAsync(
        AlertPayload payload,
        IReadOnlyDictionary<string, string> config,
        CancellationToken cancellationToken = default)
    {
        var webhookUrl = GetWebhookUrl(config);
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            logger.LogInformation(
                "[{Provider}] Stub send (no webhook URL configured): {Title}",
                ProviderId, payload.Title);
            return SendResult.Succeeded(ProviderId, $"stub-{Guid.NewGuid():N}");
        }

        try
        {
            var client = httpClientFactory.CreateClient(nameof(WebhookBasedNotificationProvider));
            var body = BuildWebhookBody(payload);
            using var response = await client.PostAsJsonAsync(webhookUrl, body, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning(
                    "[{Provider}] Webhook returned {Status}: {Error}",
                    ProviderId, response.StatusCode, error);
                return SendResult.Failed(ProviderId, $"HTTP {(int)response.StatusCode}: {error}");
            }

            return SendResult.Succeeded(ProviderId, $"webhook-{Guid.NewGuid():N}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[{Provider}] Webhook send failed", ProviderId);
            return SendResult.Failed(ProviderId, ex.Message);
        }
    }

    protected virtual string? GetWebhookUrl(IReadOnlyDictionary<string, string> config) =>
        config.GetValueOrDefault("webhookUrl")
        ?? config.GetValueOrDefault("WebhookUrl")
        ?? config.GetValueOrDefault("targetUrl")
        ?? config.GetValueOrDefault("TargetUrl");

    protected virtual object BuildWebhookBody(AlertPayload payload) =>
        new
        {
            text = $"*{payload.Title}*\n{payload.Body}",
            payload.Title,
            payload.Body,
            payload.ActionUrl,
            payload.Severity,
            payload.Metadata,
        };
}
