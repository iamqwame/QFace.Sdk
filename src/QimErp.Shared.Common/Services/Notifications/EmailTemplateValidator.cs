using System.Text.RegularExpressions;

namespace QimErp.Shared.Common.Services.Notifications;

/// <summary>
/// Validates caller token payloads and rendered HTML against <see cref="EmailTemplateCatalog"/>.
/// </summary>
public static partial class EmailTemplateValidator
{
    public sealed record ValidationResult(bool IsValid, IReadOnlyList<string> Errors)
    {
        public static ValidationResult Success() => new(true, []);
        public static ValidationResult Failure(params string[] errors) => new(false, errors);
    }

    /// <summary>
    /// Ensures every required content token is present with a non-whitespace value after merging infra defaults.
    /// </summary>
    public static ValidationResult ValidateRequiredTokens(
        string templateCode,
        IReadOnlyDictionary<string, string> mergedTokens)
    {
        var definition = EmailTemplateCatalog.TryGet(templateCode);
        if (definition is null)
            return ValidationResult.Success(); // unknown templates skip catalog validation (legacy)

        var missing = new List<string>();
        foreach (var token in definition.RequiredTokens)
        {
            if (!mergedTokens.TryGetValue(token, out var value) || string.IsNullOrWhiteSpace(value))
                missing.Add(token);
        }

        return missing.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure(
                $"Template '{templateCode}' missing required tokens: {string.Join(", ", missing)}");
    }

    /// <summary>
    /// Post-render scan — rendered body must not contain unreplaced <c>{{</c> tokens or empty avatar <c>src</c>.
    /// </summary>
    public static ValidationResult ValidateRenderedBody(string htmlBody, string templateCode)
    {
        var errors = new List<string>();

        if (htmlBody.Contains("{{", StringComparison.Ordinal))
            errors.Add($"Template '{templateCode}' rendered with unreplaced placeholders.");

        if (EmptyAvatarSrc().IsMatch(htmlBody))
            errors.Add($"Template '{templateCode}' rendered with empty avatar image src.");

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure(errors.ToArray());
    }

    [GeneratedRegex(@"<img[^>]+src=""\s*""", RegexOptions.IgnoreCase)]
    private static partial Regex EmptyAvatarSrc();
}
