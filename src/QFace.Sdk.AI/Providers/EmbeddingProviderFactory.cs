namespace QFace.Sdk.AI.Providers;

/// <summary>
/// Factory for managing and retrieving embedding providers
/// </summary>
public class EmbeddingProviderFactory
{
    private readonly Dictionary<string, IEmbeddingProvider> _providers;
    private readonly ILogger<EmbeddingProviderFactory> _logger;
    private readonly string _defaultProvider;

    public EmbeddingProviderFactory(
        IEnumerable<IEmbeddingProvider> providers,
        IOptions<AIOptions> options,
        ILogger<EmbeddingProviderFactory> logger)
    {
        _logger = logger;
        _defaultProvider = options.Value.DefaultEmbeddingProvider;
        _providers = providers.ToDictionary(p => p.ProviderName, p => p);

        _logger.LogInformation(
            "EmbeddingProviderFactory initialized with {Count} providers. Default: {DefaultProvider}",
            _providers.Count,
            _defaultProvider);
    }

    public IEmbeddingProvider GetProvider(string? providerName = null)
    {
        var name = providerName ?? _defaultProvider;

        if (!_providers.TryGetValue(name, out var provider))
        {
            var availableProviders = string.Join(", ", _providers.Keys);
            throw new ArgumentException(
                $"Embedding provider '{name}' not found. Available providers: {availableProviders}");
        }

        return provider;
    }
}
