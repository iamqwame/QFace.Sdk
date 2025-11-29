namespace QFace.Sdk.AI.Providers;

/// <summary>
/// Factory for managing and retrieving LLM providers
/// </summary>
public class LLMProviderFactory
{
    private readonly Dictionary<string, ILLMProvider> _providers;
    private readonly ILogger<LLMProviderFactory> _logger;
    private readonly string _defaultProvider;

    /// <summary>
    /// Initializes a new instance of LLMProviderFactory
    /// </summary>
    public LLMProviderFactory(
        IEnumerable<ILLMProvider> providers,
        IOptions<AIOptions> options,
        ILogger<LLMProviderFactory> logger)
    {
        _logger = logger;
        _defaultProvider = options.Value.DefaultLLMProvider;
        _providers = providers.ToDictionary(p => p.ProviderName, p => p);
        
        _logger.LogInformation("LLMProviderFactory initialized with {Count} providers. Default: {DefaultProvider}", 
            _providers.Count, _defaultProvider);
    }

    /// <summary>
    /// Gets a provider by name, or returns the default provider if name is not specified
    /// </summary>
    /// <param name="providerName">Optional provider name. If null, returns default provider.</param>
    /// <returns>The LLM provider</returns>
    /// <exception cref="ArgumentException">Thrown if provider is not found</exception>
    public ILLMProvider GetProvider(string? providerName = null)
    {
        var name = providerName ?? _defaultProvider;
        
        if (!_providers.TryGetValue(name, out var provider))
        {
            var availableProviders = string.Join(", ", _providers.Keys);
            throw new ArgumentException(
                $"LLM provider '{name}' not found. Available providers: {availableProviders}");
        }

        return provider;
    }

    /// <summary>
    /// Gets all available providers
    /// </summary>
    /// <returns>Dictionary of all providers</returns>
    public Dictionary<string, ILLMProvider> GetAllProviders()
    {
        return new Dictionary<string, ILLMProvider>(_providers);
    }

    /// <summary>
    /// Gets the default provider name
    /// </summary>
    /// <returns>Default provider name</returns>
    public string GetDefaultProviderName()
    {
        return _defaultProvider;
    }
}

