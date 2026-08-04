namespace QFace.Sdk.AI.Services;

/// <summary>
/// Service for generating text embeddings via configured providers
/// </summary>
public class EmbeddingService : IEmbeddingService
{
    private readonly EmbeddingProviderFactory _providerFactory;
    private readonly IAIOptionsProvider _optionsProvider;
    private readonly ILogger<EmbeddingService> _logger;

    public EmbeddingService(
        EmbeddingProviderFactory providerFactory,
        IAIOptionsProvider optionsProvider,
        ILogger<EmbeddingService> logger)
    {
        _providerFactory = providerFactory;
        _optionsProvider = optionsProvider;
        _logger = logger;
    }

    public async Task<EmbeddingResponse> GenerateEmbeddingAsync(
        EmbeddingRequest request,
        CancellationToken cancellationToken = default)
    {
        var options = await _optionsProvider.GetOptionsAsync(cancellationToken);
        var providerName = request.Provider ?? options.DefaultEmbeddingProvider;
        var provider = _providerFactory.GetProvider(providerName);

        _logger.LogInformation("Generating embedding using {Provider} provider", providerName);

        return await provider.GenerateEmbeddingAsync(request, cancellationToken);
    }
}
