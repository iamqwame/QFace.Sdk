using QimErp.Shared.Common.Notifications.Providers;

namespace QimErp.Shared.Common.Notifications.Extensions;

public static class NotificationChannelServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationChannelProviders(this IServiceCollection services)
    {
        services.AddHttpClient(nameof(WebhookBasedNotificationProvider));
        services.AddHttpClient(nameof(SmsNotificationProvider));

        services.AddScoped<INotificationChannelProvider, SlackNotificationProvider>();
        services.AddScoped<INotificationChannelProvider, TeamsNotificationProvider>();
        services.AddScoped<INotificationChannelProvider, DiscordNotificationProvider>();
        services.AddScoped<INotificationChannelProvider, SmsNotificationProvider>();
        services.AddScoped<INotificationChannelProvider, GoogleMeetConferencingProvider>();
        services.AddScoped<INotificationChannelProvider, ZoomConferencingProvider>();
        services.AddScoped<INotificationChannelProvider, TeamsMeetingConferencingProvider>();
        services.AddScoped<INotificationChannelProvider, WebhookNotificationProvider>();

        services.AddScoped<INotificationDispatcher, NotificationDispatcher>();

        return services;
    }
}
