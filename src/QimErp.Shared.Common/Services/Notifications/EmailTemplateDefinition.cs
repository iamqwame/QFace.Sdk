namespace QimErp.Shared.Common.Services.Notifications;

/// <summary>
/// Manifest entry for a design-system email template (S3 slug without <c>.html</c>).
/// Infrastructure tokens (Year, Portal, SupportEmail, Company, CompanyFooter) are merged by
/// the notification service and are not listed here.
/// </summary>
public sealed record EmailTemplateDefinition(
    string TemplateCode,
    IReadOnlyList<string> RequiredTokens,
    IReadOnlyList<string> OptionalTokens);
