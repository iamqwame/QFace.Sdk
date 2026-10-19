namespace QimErp.Shared.Common.Services.Notifications;

public static class EmailTemplatePlaceholderSubstitutor
{
    public static string Substitute(string template, IReadOnlyDictionary<string, string> replacements)
    {
        if (string.IsNullOrEmpty(template) || replacements is null || replacements.Count == 0)
            return template;

        var body = template;
        foreach (var (key, value) in replacements)
            body = body.Replace("{{" + key + "}}", value ?? string.Empty, StringComparison.Ordinal);

        return body;
    }
}
