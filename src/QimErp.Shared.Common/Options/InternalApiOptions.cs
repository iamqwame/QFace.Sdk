namespace QimErp.Shared.Common.Options;

/// <summary>
/// Configuration for internal API authentication (service-to-service calls from Platform Orchestration).
/// </summary>
public class InternalApiOptions
{
    public const string SectionName = "InternalApi";

    /// <summary>
    /// Expected value of X-Internal-Api-Key header. Empty rejects every request guarded by
    /// <c>InternalApiAuthFilter</c>; a service exposing /internal routes must configure it.
    /// </summary>
    public string ExpectedApiKey { get; set; } = string.Empty;
}
