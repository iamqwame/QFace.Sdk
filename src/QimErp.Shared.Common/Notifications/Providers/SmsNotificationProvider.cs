using System.Net.Http.Json;
using QimErp.Shared.Common.Notifications.Models;

namespace QimErp.Shared.Common.Notifications.Providers;

public sealed class SmsNotificationProvider(
    IHttpClientFactory httpClientFactory,
    ILogger<SmsNotificationProvider> logger) : INotificationChannelProvider
{
    public NotificationProviderId ProviderId => NotificationProviderId.Sms;
    public NotificationChannelFamily Family => NotificationChannelFamily.Text;
    public string PluginKey => NotificationProviderMetadata.GetPluginKey(ProviderId);
    public int DispatchOrder => NotificationProviderMetadata.GetDispatchOrder(ProviderId);

    public Task<MeetingResult?> CreateMeetingAsync(
        MeetingRequest request,
        IReadOnlyDictionary<string, string> config,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<MeetingResult?>(null);

    public async Task<SendResult> SendAlertAsync(
        AlertPayload payload,
        IReadOnlyDictionary<string, string> config,
        CancellationToken cancellationToken = default)
    {
        var gatewayUrl = config.GetValueOrDefault("gatewayUrl")
            ?? config.GetValueOrDefault("GatewayUrl")
            ?? config.GetValueOrDefault("smsGatewayUrl")
            ?? config.GetValueOrDefault("SmsGatewayUrl");

        var senderId = config.GetValueOrDefault("senderId")
            ?? config.GetValueOrDefault("SenderId")
            ?? "QimERP";

        if (string.IsNullOrWhiteSpace(gatewayUrl))
        {
            logger.LogInformation(
                "[Sms] Stub send (no gateway URL configured): {Title} via {SenderId}",
                payload.Title, senderId);
            return SendResult.Succeeded(ProviderId, $"sms-stub-{Guid.NewGuid():N}");
        }

        try
        {
            var client = httpClientFactory.CreateClient(nameof(SmsNotificationProvider));
            var body = new
            {
                sender = senderId,
                message = $"{payload.Title}: {payload.Body}",
                payload.ActionUrl,
            };

            using var response = await client.PostAsJsonAsync(gatewayUrl, body, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning("[Sms] Gateway returned {Status}: {Error}", response.StatusCode, error);
                return SendResult.Failed(ProviderId, $"HTTP {(int)response.StatusCode}: {error}");
            }

            return SendResult.Succeeded(ProviderId, $"sms-{Guid.NewGuid():N}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[Sms] Gateway send failed");
            return SendResult.Failed(ProviderId, ex.Message);
        }
    }
}
