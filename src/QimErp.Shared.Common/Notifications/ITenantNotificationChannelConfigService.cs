using QimErp.Shared.Common.Notifications.Models;

namespace QimErp.Shared.Common.Notifications;

public interface ITenantNotificationChannelConfigService
{
    Task<PluginNotificationSettings> GetPluginSettingsAsync(
        string pluginKey,
        CancellationToken cancellationToken = default);

    Task SavePluginSettingsAsync(
        string pluginKey,
        PluginNotificationSettings settings,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NotificationProviderSettings>> GetEnabledProvidersAsync(
        string? tenantId = null,
        CancellationToken cancellationToken = default);

    Task<NotificationProviderSettings?> GetProviderSettingsAsync(
        string pluginKey,
        NotificationProviderId providerId,
        CancellationToken cancellationToken = default);
}
