namespace QFace.Sdk.AI.Services;

/// <summary>
/// Service for interacting with LLM providers
/// </summary>
public class LLMService : ILLMService
{
    private readonly LLMProviderFactory _providerFactory;
    private readonly IAIOptionsProvider _optionsProvider;
    private readonly ILogger<LLMService> _logger;

    /// <summary>
    /// Initializes a new instance of LLMService
    /// </summary>
    public LLMService(
        LLMProviderFactory providerFactory,
        IAIOptionsProvider optionsProvider,
        ILogger<LLMService> logger)
    {
        _providerFactory = providerFactory;
        _optionsProvider = optionsProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<LLMResponse> GenerateCompletionAsync(LLMRequest request, CancellationToken cancellationToken = default)
    {
        var options = await _optionsProvider.GetOptionsAsync(cancellationToken);
        var providerName = request.Provider ?? options.DefaultLLMProvider;
        var provider = _providerFactory.GetProvider(providerName);

        _logger.LogInformation("Generating completion using {Provider} provider", providerName);

        return await provider.GenerateCompletionAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<LLMResponse> GenerateChatCompletionAsync(LLMRequest request, CancellationToken cancellationToken = default)
    {
        var options = await _optionsProvider.GetOptionsAsync(cancellationToken);
        var providerName = request.Provider ?? options.DefaultLLMProvider;
        var provider = _providerFactory.GetProvider(providerName);

        _logger.LogInformation("Generating chat completion using {Provider} provider", providerName);

        return await provider.GenerateChatCompletionAsync(request, cancellationToken);
    }
}

