namespace QimErp.Shared.Common.Notifications.Models;

public sealed class SendResult
{
    public NotificationProviderId ProviderId { get; set; }
    public bool Success { get; set; }
    public string? MessageId { get; set; }
    public string? Error { get; set; }
    public bool UsedFallback { get; set; }

    public static SendResult Succeeded(NotificationProviderId providerId, string? messageId = null) =>
        new() { ProviderId = providerId, Success = true, MessageId = messageId };

    public static SendResult Failed(NotificationProviderId providerId, string error) =>
        new() { ProviderId = providerId, Success = false, Error = error };
}
