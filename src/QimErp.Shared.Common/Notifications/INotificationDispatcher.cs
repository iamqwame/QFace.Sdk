using QimErp.Shared.Common.Notifications.Models;

namespace QimErp.Shared.Common.Notifications;

public interface INotificationDispatcher
{
    Task<IReadOnlyList<SendResult>> DispatchAlertAsync(
        string tenantId,
        AlertPayload payload,
        bool smsFallbackToEmail = true,
        CancellationToken cancellationToken = default);

    Task<SendResult> DispatchTestAlertAsync(
        string tenantId,
        NotificationProviderId providerId,
        AlertPayload payload,
        CancellationToken cancellationToken = default);

    Task<MeetingResult?> CreateMeetingAsync(
        string tenantId,
        NotificationProviderId providerId,
        MeetingRequest request,
        CancellationToken cancellationToken = default);
}
