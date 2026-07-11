using QimErp.Shared.Common.TenantSetup;

namespace QimErp.Shared.Common.Notifications;

public static class NotificationProviderMetadata
{
    public static NotificationChannelFamily GetFamily(NotificationProviderId providerId) =>
        providerId switch
        {
            NotificationProviderId.Slack or NotificationProviderId.MicrosoftTeams or NotificationProviderId.Discord
                => NotificationChannelFamily.TeamChat,
            NotificationProviderId.Sms => NotificationChannelFamily.Text,
            NotificationProviderId.GoogleMeet or NotificationProviderId.Zoom or NotificationProviderId.MicrosoftTeamsMeeting
                => NotificationChannelFamily.Conferencing,
            NotificationProviderId.Webhook => NotificationChannelFamily.Webhook,
            _ => throw new ArgumentOutOfRangeException(nameof(providerId), providerId, "Unknown provider."),
        };

    public static string GetPluginKey(NotificationProviderId providerId) =>
        providerId switch
        {
            NotificationProviderId.Slack or NotificationProviderId.MicrosoftTeams or NotificationProviderId.Discord
                => PluginKeys.ChatNotify,
            NotificationProviderId.Sms => PluginKeys.SmsNotify,
            NotificationProviderId.GoogleMeet or NotificationProviderId.Zoom or NotificationProviderId.MicrosoftTeamsMeeting
                => PluginKeys.ConferenceNotify,
            NotificationProviderId.Webhook => PluginKeys.WebhookNotify,
            _ => throw new ArgumentOutOfRangeException(nameof(providerId), providerId, "Unknown provider."),
        };

    public static int GetDispatchOrder(NotificationProviderId providerId) =>
        providerId switch
        {
            NotificationProviderId.Slack => 10,
            NotificationProviderId.MicrosoftTeams => 20,
            NotificationProviderId.Discord => 30,
            NotificationProviderId.Sms => 40,
            NotificationProviderId.GoogleMeet => 50,
            NotificationProviderId.Zoom => 60,
            NotificationProviderId.MicrosoftTeamsMeeting => 70,
            NotificationProviderId.Webhook => 80,
            _ => 100,
        };
}
