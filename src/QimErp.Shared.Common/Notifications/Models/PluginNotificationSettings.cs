namespace QimErp.Shared.Common.Notifications.Models;

public sealed class PluginNotificationSettings
{
    public bool Enabled { get; set; } = true;
    public bool FallbackToEmail { get; set; } = true;
    public List<NotificationProviderSettings> Providers { get; set; } = [];
}
