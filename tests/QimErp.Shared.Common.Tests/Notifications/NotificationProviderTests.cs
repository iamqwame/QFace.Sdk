using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using QimErp.Shared.Common.Notifications;
using QimErp.Shared.Common.Notifications.Models;
using QimErp.Shared.Common.Notifications.Providers;
using Xunit;

namespace QimErp.Shared.Common.Tests.Notifications;

public class NotificationProviderTests
{
    [Fact]
    public async Task SlackProvider_WithoutWebhook_ReturnsStubSuccess()
    {
        var provider = new SlackNotificationProvider(
            new StubHttpClientFactory(),
            NullLogger<SlackNotificationProvider>.Instance);

        var result = await provider.SendAlertAsync(
            new AlertPayload { Title = "Approval", Body = "Please review" },
            new Dictionary<string, string>());

        result.Success.Should().BeTrue();
        result.ProviderId.Should().Be(NotificationProviderId.Slack);
        result.MessageId.Should().StartWith("stub-");
    }

    [Fact]
    public async Task SmsProvider_WithoutGateway_ReturnsStubSuccess()
    {
        var provider = new SmsNotificationProvider(
            new StubHttpClientFactory(),
            NullLogger<SmsNotificationProvider>.Instance);

        var result = await provider.SendAlertAsync(
            new AlertPayload { Title = "Reminder", Body = "Action required" },
            new Dictionary<string, string> { ["senderId"] = "QimERP" });

        result.Success.Should().BeTrue();
        result.ProviderId.Should().Be(NotificationProviderId.Sms);
        result.MessageId.Should().StartWith("sms-stub-");
    }

    [Fact]
    public async Task GoogleMeetProvider_CreateMeeting_ReturnsStubLink()
    {
        var provider = new GoogleMeetConferencingProvider(NullLogger<GoogleMeetConferencingProvider>.Instance);

        var result = await provider.CreateMeetingAsync(
            new MeetingRequest { Title = "Interview", StartTime = DateTimeOffset.UtcNow },
            new Dictionary<string, string>());

        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.MeetingUrl.Should().StartWith("https://meet.google.com/stub-");
    }

    private sealed class StubHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new();
    }
}
