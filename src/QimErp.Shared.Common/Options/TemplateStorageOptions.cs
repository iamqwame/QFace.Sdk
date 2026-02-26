namespace QimErp.Shared.Common.Options;

/// <summary>
/// Configuration options for template storage in S3.
/// </summary>
public class TemplateStorageOptions
{
    public const string SectionName = "TemplateStorage";

    /// <summary>
    /// S3 prefix for email templates (e.g. "templates/emails").
    /// </summary>
    public string Prefix { get; set; } = "templates/emails";

    /// <summary>
    /// Cache TTL in minutes. When 0, a default of 15 minutes is used (Redis requires a TTL).
    /// </summary>
    public int CacheMinutes { get; set; } = 15;
}
