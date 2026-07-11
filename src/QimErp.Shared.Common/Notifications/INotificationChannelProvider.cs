using QimErp.Shared.Common.Notifications.Models;

namespace QimErp.Shared.Common.Notifications;

public interface INotificationChannelProvider
{
    NotificationProviderId ProviderId { get; }
    NotificationChannelFamily Family { get; }
    string PluginKey { get; }
    int DispatchOrder { get; }

    Task<SendResult> SendAlertAsync(
        AlertPayload payload,
        IReadOnlyDictionary<string, string> config,
        CancellationToken cancellationToken = default);

    Task<MeetingResult?> CreateMeetingAsync(
        MeetingRequest request,
        IReadOnlyDictionary<string, string> config,
        CancellationToken cancellationToken = default);
}
