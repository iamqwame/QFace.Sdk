namespace QFace.Sdk.BlobStorage.Models;

/// <summary>
/// Extension methods for BlobStorageOptions.
/// </summary>
public static class BlobStorageOptionsExtensions
{
    /// <summary>
    /// Gets the effective template storage prefix, with fallback to default if null or empty.
    /// </summary>
    public static string GetTemplateStoragePrefix(this BlobStorageOptions options) =>
        string.IsNullOrEmpty(options.TemplateStorage.Prefix.TrimEnd('/'))
            ? "templates/emails"
            : options.TemplateStorage.Prefix.TrimEnd('/');
}
