namespace QFace.Sdk.AI.Services;

/// <summary>
/// Default IAIOptionsProvider — reads options straight from local configuration.
/// Registered with TryAdd so QimErp.Shared.Common's cache-backed provider can
/// take over when the host also calls AddCoreServices (the normal case).
/// </summary>
public class LocalAIOptionsProvider : IAIOptionsProvider
{
    private readonly AIOptions _options;

    /// <summary>
    /// Initializes a new instance of LocalAIOptionsProvider.
    /// </summary>
    public LocalAIOptionsProvider(IOptions<AIOptions> options)
    {
        _options = options.Value;
    }

    /// <inheritdoc />
    public Task<AIOptions> GetOptionsAsync(CancellationToken cancellationToken = default) => Task.FromResult(_options);
}
