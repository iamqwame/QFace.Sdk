namespace QFace.Sdk.AI.Models;

/// <summary>
/// Request for embedding generation
/// </summary>
public class EmbeddingRequest
{
    /// <summary>
    /// Text to embed
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Optional provider override (GoogleGemini)
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// Optional model override
    /// </summary>
    public string? Model { get; set; }
}

/// <summary>
/// Response from embedding generation
/// </summary>
public class EmbeddingResponse
{
    /// <summary>
    /// Embedding vector values
    /// </summary>
    public float[] Embedding { get; set; } = [];

    /// <summary>
    /// Provider that generated the embedding
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Model that generated the embedding
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Dimension count of the embedding vector
    /// </summary>
    public int Dimensions => Embedding.Length;
}
