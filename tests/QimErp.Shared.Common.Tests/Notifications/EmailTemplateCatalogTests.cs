using FluentAssertions;
using QimErp.Shared.Common.Services.Notifications;
using Xunit;

namespace QimErp.Shared.Common.Tests.Notifications;

public class EmailTemplateCatalogTests
{
    [Theory]
    [InlineData("approval-request", "AvatarUrl")]
    [InlineData("payroll-payslip", "AvatarUrl")]
    [InlineData("login-success", "FirstName")]
    public void Catalog_ShouldRequireExpectedTokens(string templateCode, string token)
    {
        var definition = EmailTemplateCatalog.Get(templateCode);
        definition.RequiredTokens.Should().Contain(token);
    }

    [Fact]
    public void Catalog_ShouldContainAllTwentyTemplates()
    {
        EmailTemplateCatalog.All.Should().HaveCount(20);
    }

    [Fact]
    public void AvatarUrl_ShouldBeRequired_WhenTemplateUsesAvatarInCatalog()
    {
        var avatarTemplates = new[]
        {
            "approval-request", "approval-approved", "approval-rejected",
            "approval-reminder", "approval-stage-active", "approval-stage-advanced",
            "leave-approved", "leave-rejected", "payroll-payslip", "account-welcome",
            "onboarding-welcome",
        };

        foreach (var code in avatarTemplates)
        {
            EmailTemplateCatalog.Get(code).RequiredTokens.Should().Contain("AvatarUrl", code);
        }
    }

    [Fact]
    public void Validator_ShouldRejectMissingRequiredTokens()
    {
        var result = EmailTemplateValidator.ValidateRequiredTokens(
            "approval-request",
            new Dictionary<string, string> { ["ItemTitle"] = "Test" });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("AvatarUrl"));
    }

    [Fact]
    public void Validator_ShouldRejectUnreplacedPlaceholdersInRenderedBody()
    {
        var result = EmailTemplateValidator.ValidateRenderedBody(
            "<p>Hello {{FirstName}}</p>",
            "approval-request");

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ResolveAvatarUrl_ShouldFallbackToPlatformDefault_WhenProfileMissing()
    {
        EmailAvatarTokens.ResolveAvatarUrl(null, "https://app.example.com/static/avatar-placeholder.png")
            .Should().Be("https://app.example.com/static/avatar-placeholder.png");
    }
}
