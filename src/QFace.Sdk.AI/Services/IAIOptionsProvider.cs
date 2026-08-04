namespace QFace.Sdk.AI.Services;

/// <summary>
/// Resolves the current AIOptions — implementations may read from local config,
/// a shared cache, or both.
/// </summary>
public interface IAIOptionsProvider
{
    /// <summary>
    /// Gets the current AI provider configuration.
    /// </summary>
    Task<AIOptions> GetOptionsAsync(CancellationToken cancellationToken = default);
}
