namespace QimErp.Shared.Common.Notifications.Models;

public sealed class AlertPayload
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public string? Severity { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}
