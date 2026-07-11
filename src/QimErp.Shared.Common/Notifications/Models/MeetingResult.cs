namespace QimErp.Shared.Common.Notifications.Models;

public sealed class MeetingResult
{
    public NotificationProviderId ProviderId { get; set; }
    public bool Success { get; set; }
    public string? MeetingUrl { get; set; }
    public string? MeetingId { get; set; }
    public string? Error { get; set; }

    public static MeetingResult Succeeded(NotificationProviderId providerId, string meetingUrl, string? meetingId = null) =>
        new() { ProviderId = providerId, Success = true, MeetingUrl = meetingUrl, MeetingId = meetingId };

    public static MeetingResult Failed(NotificationProviderId providerId, string error) =>
        new() { ProviderId = providerId, Success = false, Error = error };
}
