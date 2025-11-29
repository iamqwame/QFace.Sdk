using Google.GenerativeAI;
using Google.GenerativeAI.Clients;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using QFace.Sdk.AI.Models;

namespace QFace.Sdk.AI.Providers;

/// <summary>
/// Google Gemini LLM provider implementation
/// </summary>
public class GoogleGeminiProvider : ILLMProvider
{
    private readonly GoogleGeminiOptions _options;
    private readonly ILogger<GoogleGeminiProvider> _logger;
    private GenerativeModel? _model;
    private bool _initialized;

    /// <summary>
    /// Provider name
    /// </summary>
    public string ProviderName => "GoogleGemini";

    /// <summary>
    /// Initializes a new instance of GoogleGeminiProvider
    /// </summary>
    public GoogleGeminiProvider(IOptions<AIOptions> aiOptions, ILogger<GoogleGeminiProvider> logger)
    {
        _options = aiOptions.Value.GoogleGemini;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> InitializeAsync()
    {
        if (_initialized)
        {
            return true;
        }

        try
        {
            if (string.IsNullOrEmpty(_options.ApiKey))
            {
                _logger.LogWarning("Google Gemini API key is not configured");
                return false;
            }

            var client = new GoogleAI(_options.ApiKey);
            var modelName = _options.DefaultModel;
            _model = client.GetGenerativeModel(modelName);
            _initialized = true;
            _logger.LogInformation("Google Gemini provider initialized successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Google Gemini provider");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<LLMResponse> GenerateCompletionAsync(LLMRequest request, CancellationToken cancellationToken = default)
    {
        if (!_initialized)
        {
            await InitializeAsync();
        }

        if (_model == null)
        {
            throw new InvalidOperationException("Google Gemini model is not initialized");
        }

        try
        {
            var modelName = request.Model ?? _options.DefaultModel;
            var maxTokens = request.MaxTokens ?? _options.MaxTokens;

            var response = await _model.GenerateContentAsync(request.Prompt, cancellationToken: cancellationToken);

            var content = response.Text ?? string.Empty;
            var tokensUsed = response.UsageMetadata?.TotalTokenCount ?? 0;

            return new LLMResponse
            {
                Content = content,
                Provider = ProviderName,
                Model = modelName,
                TokensUsed = tokensUsed,
                Metadata = new Dictionary<string, object>
                {
                    { "FinishReason", response.Candidates?.FirstOrDefault()?.FinishReason?.ToString() ?? "unknown" }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Google Gemini completion");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<LLMResponse> GenerateChatCompletionAsync(LLMRequest request, CancellationToken cancellationToken = default)
    {
        if (!_initialized)
        {
            await InitializeAsync();
        }

        if (_model == null)
        {
            throw new InvalidOperationException("Google Gemini model is not initialized");
        }

        try
        {
            var modelName = request.Model ?? _options.DefaultModel;
            var maxTokens = request.MaxTokens ?? _options.MaxTokens;

            // Build chat history if messages are provided
            var chat = _model.StartChat();
            
            if (request.Messages != null && request.Messages.Count > 0)
            {
                foreach (var message in request.Messages)
                {
                    if (message.Role.ToLower() == "user")
                    {
                        await chat.SendMessageAsync(message.Content, cancellationToken: cancellationToken);
                    }
                    else if (message.Role.ToLower() == "assistant")
                    {
                        // Add assistant message to history
                        // Note: Google Gemini SDK handles this internally through chat history
                    }
                }
            }

            var response = await chat.SendMessageAsync(request.Prompt, cancellationToken: cancellationToken);

            var content = response.Text ?? string.Empty;
            var tokensUsed = response.UsageMetadata?.TotalTokenCount ?? 0;

            return new LLMResponse
            {
                Content = content,
                Provider = ProviderName,
                Model = modelName,
                TokensUsed = tokensUsed,
                Metadata = new Dictionary<string, object>
                {
                    { "FinishReason", response.Candidates?.FirstOrDefault()?.FinishReason?.ToString() ?? "unknown" }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Google Gemini chat completion");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> IsAvailableAsync()
    {
        if (!_initialized)
        {
            return await InitializeAsync();
        }

        return _model != null && !string.IsNullOrEmpty(_options.ApiKey);
    }
}

