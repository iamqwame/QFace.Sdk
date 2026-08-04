using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using QFace.Sdk.AI.Models;
using QFace.Sdk.AI.Services;

namespace QFace.Sdk.AI.Providers;

/// <summary>
/// Anthropic LLM provider implementation using Microsoft.Extensions.AI
/// NOTE: Microsoft.Extensions.AI.Anthropic package may not be available yet.
/// This is a stub implementation that will work when the package is available.
/// </summary>
public class AnthropicProvider : ILLMProvider
{
    private readonly IAIOptionsProvider _optionsProvider;
    private readonly ILogger<AnthropicProvider> _logger;

    /// <summary>
    /// Provider name
    /// </summary>
    public string ProviderName => "Anthropic";

    /// <summary>
    /// Initializes a new instance of AnthropicProvider
    /// </summary>
    public AnthropicProvider(IAIOptionsProvider optionsProvider, ILogger<AnthropicProvider> logger)
    {
        _optionsProvider = optionsProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> InitializeAsync()
    {
        var options = (await _optionsProvider.GetOptionsAsync()).Anthropic;
        if (string.IsNullOrEmpty(options.ApiKey))
        {
            _logger.LogWarning("Anthropic API key is not configured");
            return false;
        }

        _logger.LogWarning("Anthropic provider via Microsoft.Extensions.AI is not yet fully implemented. " +
                          "Microsoft.Extensions.AI.Anthropic package may not be available.");
        return true;
    }

    /// <inheritdoc />
    public Task<LLMResponse> GenerateCompletionAsync(LLMRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(
            "Anthropic provider implementation needs Microsoft.Extensions.AI.Anthropic package. " +
            "This package may not be available yet. Please check Microsoft.Extensions.AI documentation for Anthropic support.");
    }

    /// <inheritdoc />
    public Task<LLMResponse> GenerateChatCompletionAsync(LLMRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(
            "Anthropic provider implementation needs Microsoft.Extensions.AI.Anthropic package. " +
            "This package may not be available yet. Please check Microsoft.Extensions.AI documentation for Anthropic support.");
    }

    /// <inheritdoc />
    public async Task<bool> IsAvailableAsync()
    {
        var options = (await _optionsProvider.GetOptionsAsync()).Anthropic;
        return !string.IsNullOrEmpty(options.ApiKey);
    }
}
