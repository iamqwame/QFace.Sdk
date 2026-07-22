namespace QFace.Sdk.AI.Services;

/// <summary>
/// Service for generating text embeddings via configured providers
/// </summary>
public class EmbeddingService : IEmbeddingService
{
    private readonly EmbeddingProviderFactory _providerFactory;
    private readonly AIOptions _options;
    private readonly ILogger<EmbeddingService> _logger;

    public EmbeddingService(
        EmbeddingProviderFactory providerFactory,
        IOptions<AIOptions> options,
        ILogger<EmbeddingService> logger)
    {
        _providerFactory = providerFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<EmbeddingResponse> GenerateEmbeddingAsync(
        EmbeddingRequest request,
        CancellationToken cancellationToken = default)
    {
        var providerName = request.Provider ?? _options.DefaultEmbeddingProvider;
        var provider = _providerFactory.GetProvider(providerName);

        _logger.LogInformation("Generating embedding using {Provider} provider", providerName);

        return await provider.GenerateEmbeddingAsync(request, cancellationToken);
    }
}
