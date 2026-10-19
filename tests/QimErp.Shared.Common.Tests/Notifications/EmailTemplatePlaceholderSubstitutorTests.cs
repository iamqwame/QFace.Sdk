using FluentAssertions;
using QimErp.Shared.Common.Events;
using QimErp.Shared.Common.Services.Notifications;
using Xunit;

namespace QimErp.Shared.Common.Tests.Notifications;

public class EmailTemplatePlaceholderSubstitutorTests
{
    [Fact]
    public void Substitute_replaces_known_placeholders()
    {
        var html = EmailTemplatePlaceholderSubstitutor.Substitute(
            "Hi {{FirstName}} — <a href=\"{{UnlockUrl}}\">go</a>",
            new Dictionary<string, string>
            {
                ["FirstName"] = "Ada",
                ["UnlockUrl"] = "https://example.com/unlock"
            });

        html.Should().Be("Hi Ada — <a href=\"https://example.com/unlock\">go</a>");
    }

    [Fact]
    public void ReplacementKeyFor_maps_purposes()
    {
        EmailLinkMetadata.ReplacementKeyFor(EmailLinkPurpose.PasswordReset).Should().Be("Link");
        EmailLinkMetadata.ReplacementKeyFor(EmailLinkPurpose.AccountActivation).Should().Be("Link");
        EmailLinkMetadata.ReplacementKeyFor(EmailLinkPurpose.AccountUnlock).Should().Be("UnlockUrl");
        EmailLinkMetadata.ReplacementKeyFor(EmailLinkPurpose.SetPasswordInvite).Should().Be("SetPasswordUrl");
    }
}
