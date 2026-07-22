namespace QFace.Sdk.AI.Providers;

/// <summary>
/// Interface for embedding providers
/// </summary>
public interface IEmbeddingProvider
{
    /// <summary>
    /// Provider name (e.g., "GoogleGemini")
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Initializes the provider with configuration
    /// </summary>
    Task<bool> InitializeAsync();

    /// <summary>
    /// Generates an embedding vector for the given text
    /// </summary>
    Task<EmbeddingResponse> GenerateEmbeddingAsync(EmbeddingRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the provider is available and configured
    /// </summary>
    Task<bool> IsAvailableAsync();
}
