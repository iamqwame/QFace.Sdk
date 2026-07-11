namespace QimErp.Shared.Common.Notifications.Models;

public sealed class MeetingRequest
{
    public string Title { get; set; } = string.Empty;
    public DateTimeOffset StartTime { get; set; }
    public TimeSpan Duration { get; set; } = TimeSpan.FromMinutes(30);
    public IReadOnlyList<string> AttendeeEmails { get; set; } = [];
    public string? Description { get; set; }
}
