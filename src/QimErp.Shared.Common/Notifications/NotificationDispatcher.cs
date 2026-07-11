using QimErp.Shared.Common.Notifications.Models;
using QimErp.Shared.Common.Services.TenantSetup;
using QimErp.Shared.Common.TenantSetup;

namespace QimErp.Shared.Common.Notifications;

public sealed class NotificationDispatcher(
    IEnumerable<INotificationChannelProvider> providers,
    ITenantNotificationChannelConfigService configService,
    ITenantPluginAccessService pluginAccess,
    ILogger<NotificationDispatcher> logger) : INotificationDispatcher
{
    private readonly IReadOnlyList<INotificationChannelProvider> _providers =
        providers.OrderBy(p => p.DispatchOrder).ToList();

    public async Task<IReadOnlyList<SendResult>> DispatchAlertAsync(
        string tenantId,
        AlertPayload payload,
        bool smsFallbackToEmail = true,
        CancellationToken cancellationToken = default)
    {
        var results = new List<SendResult>();
        var enabledProviders = await configService.GetEnabledProvidersAsync(tenantId, cancellationToken);

        foreach (var provider in _providers)
        {
            if (provider.Family == NotificationChannelFamily.Conferencing)
            {
                continue;
            }

            if (!await pluginAccess.IsPluginEnabledAsync(tenantId, provider.PluginKey, cancellationToken))
            {
                continue;
            }

            var providerConfig = enabledProviders.FirstOrDefault(p => p.Id == provider.ProviderId);
            if (providerConfig is null)
            {
                continue;
            }

            var result = await provider.SendAlertAsync(payload, providerConfig.Config, cancellationToken);

            if (!result.Success
                && provider.ProviderId == NotificationProviderId.Sms
                && smsFallbackToEmail
                && await ShouldFallbackToEmailAsync(PluginKeys.SmsNotify, cancellationToken))
            {
                logger.LogWarning(
                    "[NotificationDispatcher] SMS failed for tenant {TenantId}; email fallback flagged",
                    tenantId);
                result = new SendResult
                {
                    ProviderId = provider.ProviderId,
                    Success = false,
                    Error = result.Error,
                    UsedFallback = true,
                };
            }

            results.Add(result);
        }

        return results;
    }

    public async Task<SendResult> DispatchTestAlertAsync(
        string tenantId,
        NotificationProviderId providerId,
        AlertPayload payload,
        CancellationToken cancellationToken = default)
    {
        var provider = _providers.FirstOrDefault(p => p.ProviderId == providerId);
        if (provider is null)
        {
            return SendResult.Failed(providerId, $"No provider registered for {providerId}.");
        }

        if (!await pluginAccess.IsPluginEnabledAsync(tenantId, provider.PluginKey, cancellationToken))
        {
            return SendResult.Failed(providerId, $"Plugin '{provider.PluginKey}' is not installed.");
        }

        var pluginSettings = await configService.GetPluginSettingsAsync(provider.PluginKey, cancellationToken);
        var providerConfig = pluginSettings.Providers.FirstOrDefault(p => p.Id == providerId && p.Enabled);
        if (providerConfig is null)
        {
            return SendResult.Failed(providerId, $"Provider '{providerId}' is not enabled.");
        }

        return await provider.SendAlertAsync(payload, providerConfig.Config, cancellationToken);
    }

    public async Task<MeetingResult?> CreateMeetingAsync(
        string tenantId,
        NotificationProviderId providerId,
        MeetingRequest request,
        CancellationToken cancellationToken = default)
    {
        var provider = _providers.FirstOrDefault(p => p.ProviderId == providerId);
        if (provider is null)
        {
            return MeetingResult.Failed(providerId, $"No provider registered for {providerId}.");
        }

        if (provider.Family != NotificationChannelFamily.Conferencing)
        {
            return MeetingResult.Failed(providerId, $"Provider '{providerId}' does not support meetings.");
        }

        if (!await pluginAccess.IsPluginEnabledAsync(tenantId, provider.PluginKey, cancellationToken))
        {
            return MeetingResult.Failed(providerId, $"Plugin '{provider.PluginKey}' is not installed.");
        }

        var pluginSettings = await configService.GetPluginSettingsAsync(provider.PluginKey, cancellationToken);
        var providerConfig = pluginSettings.Providers.FirstOrDefault(p => p.Id == providerId && p.Enabled);
        if (providerConfig is null)
        {
            return MeetingResult.Failed(providerId, $"Provider '{providerId}' is not enabled.");
        }

        return await provider.CreateMeetingAsync(request, providerConfig.Config, cancellationToken);
    }

    private async Task<bool> ShouldFallbackToEmailAsync(string pluginKey, CancellationToken cancellationToken)
    {
        var settings = await configService.GetPluginSettingsAsync(pluginKey, cancellationToken);
        return settings.FallbackToEmail;
    }
}
