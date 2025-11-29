namespace QFace.Sdk.AI.Services;

/// <summary>
/// Service for interacting with LLM providers
/// </summary>
public class LLMService : ILLMService
{
    private readonly LLMProviderFactory _providerFactory;
    private readonly AIOptions _options;
    private readonly ILogger<LLMService> _logger;

    /// <summary>
    /// Initializes a new instance of LLMService
    /// </summary>
    public LLMService(
        LLMProviderFactory providerFactory,
        IOptions<AIOptions> options,
        ILogger<LLMService> logger)
    {
        _providerFactory = providerFactory;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<LLMResponse> GenerateCompletionAsync(LLMRequest request, CancellationToken cancellationToken = default)
    {
        var providerName = request.Provider ?? _options.DefaultLLMProvider;
        var provider = _providerFactory.GetProvider(providerName);
        
        _logger.LogInformation("Generating completion using {Provider} provider", providerName);
        
        return await provider.GenerateCompletionAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<LLMResponse> GenerateChatCompletionAsync(LLMRequest request, CancellationToken cancellationToken = default)
    {
        var providerName = request.Provider ?? _options.DefaultLLMProvider;
        var provider = _providerFactory.GetProvider(providerName);
        
        _logger.LogInformation("Generating chat completion using {Provider} provider", providerName);
        
        return await provider.GenerateChatCompletionAsync(request, cancellationToken);
    }
}

