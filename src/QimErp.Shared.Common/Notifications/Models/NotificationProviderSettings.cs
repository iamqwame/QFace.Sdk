namespace QimErp.Shared.Common.Notifications.Models;

public sealed class NotificationProviderSettings
{
    public NotificationProviderId Id { get; set; }
    public bool Enabled { get; set; }
    public Dictionary<string, string> Config { get; set; } = new();
}
