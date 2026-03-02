namespace QimErp.Shared.Common.Options;

/// <summary>
/// Configuration for internal API authentication (service-to-service calls from Platform Orchestration).
/// </summary>
public class InternalApiOptions
{
    public const string SectionName = "InternalApi";

    /// <summary>
    /// Expected value of X-Internal-Api-Key header. If empty, internal API auth is disabled (not recommended for production).
    /// </summary>
    public string ExpectedApiKey { get; set; } = string.Empty;
}
