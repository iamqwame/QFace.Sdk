using QimErp.Shared.Common.Notifications.Models;

namespace QimErp.Shared.Common.Notifications.Providers;

public abstract class ConferencingProviderBase(ILogger logger) : INotificationChannelProvider
{
    public abstract NotificationProviderId ProviderId { get; }
    public NotificationChannelFamily Family => NotificationChannelFamily.Conferencing;
    public string PluginKey => NotificationProviderMetadata.GetPluginKey(ProviderId);
    public int DispatchOrder => NotificationProviderMetadata.GetDispatchOrder(ProviderId);

    public Task<SendResult> SendAlertAsync(
        AlertPayload payload,
        IReadOnlyDictionary<string, string> config,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "[{Provider}] Alert not supported for conferencing provider: {Title}",
            ProviderId, payload.Title);
        return Task.FromResult(SendResult.Failed(ProviderId, "Conferencing providers do not send alerts."));
    }

    public Task<MeetingResult?> CreateMeetingAsync(
        MeetingRequest request,
        IReadOnlyDictionary<string, string> config,
        CancellationToken cancellationToken = default)
    {
        var meetingId = Guid.NewGuid().ToString("N")[..12];
        var meetingUrl = BuildStubMeetingUrl(meetingId);
        logger.LogInformation(
            "[{Provider}] Stub meeting created: {Title} -> {MeetingUrl}",
            ProviderId, request.Title, meetingUrl);
        return Task.FromResult<MeetingResult?>(MeetingResult.Succeeded(ProviderId, meetingUrl, meetingId));
    }

    protected abstract string BuildStubMeetingUrl(string meetingId);
}

public sealed class GoogleMeetConferencingProvider(ILogger<GoogleMeetConferencingProvider> logger)
    : ConferencingProviderBase(logger)
{
    public override NotificationProviderId ProviderId => NotificationProviderId.GoogleMeet;

    protected override string BuildStubMeetingUrl(string meetingId) =>
        $"https://meet.google.com/stub-{meetingId}";
}

public sealed class ZoomConferencingProvider(ILogger<ZoomConferencingProvider> logger)
    : ConferencingProviderBase(logger)
{
    public override NotificationProviderId ProviderId => NotificationProviderId.Zoom;

    protected override string BuildStubMeetingUrl(string meetingId) =>
        $"https://zoom.us/j/stub{meetingId}";
}

public sealed class TeamsMeetingConferencingProvider(ILogger<TeamsMeetingConferencingProvider> logger)
    : ConferencingProviderBase(logger)
{
    public override NotificationProviderId ProviderId => NotificationProviderId.MicrosoftTeamsMeeting;

    protected override string BuildStubMeetingUrl(string meetingId) =>
        $"https://teams.microsoft.com/l/meetup-join/stub-{meetingId}";
}
