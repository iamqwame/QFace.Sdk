namespace QFace.Sdk.AI.Services;

/// <summary>
/// Service for generating text embeddings
/// </summary>
public interface IEmbeddingService
{
    /// <summary>
    /// Generates an embedding vector for the given text
    /// </summary>
    Task<EmbeddingResponse> GenerateEmbeddingAsync(EmbeddingRequest request, CancellationToken cancellationToken = default);
}
