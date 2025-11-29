using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using QFace.Sdk.AI.Models;

namespace QFace.Sdk.AI.Providers;

/// <summary>
/// Anthropic LLM provider implementation
/// </summary>
public class AnthropicProvider : ILLMProvider
{
    private readonly AnthropicOptions _options;
    private readonly ILogger<AnthropicProvider> _logger;
    private AnthropicClient? _client;
    private bool _initialized;

    /// <summary>
    /// Provider name
    /// </summary>
    public string ProviderName => "Anthropic";

    /// <summary>
    /// Initializes a new instance of AnthropicProvider
    /// </summary>
    public AnthropicProvider(IOptions<AIOptions> aiOptions, ILogger<AnthropicProvider> logger)
    {
        _options = aiOptions.Value.Anthropic;
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
                _logger.LogWarning("Anthropic API key is not configured");
                return false;
            }

            _client = new AnthropicClient(_options.ApiKey);
            _initialized = true;
            _logger.LogInformation("Anthropic provider initialized successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Anthropic provider");
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

        if (_client == null)
        {
            throw new InvalidOperationException("Anthropic client is not initialized");
        }

        try
        {
            var model = request.Model ?? _options.DefaultModel;
            var maxTokens = request.MaxTokens ?? _options.MaxTokens;

            var messages = new List<Message>
            {
                new Message { Role = "user", Content = request.Prompt }
            };

            var messageRequest = new MessagesRequest
            {
                Model = model,
                MaxTokens = maxTokens,
                Messages = messages
            };

            var response = await _client.Messages.CreateAsync(messageRequest, cancellationToken);

            var content = response.Content.FirstOrDefault()?.Text ?? string.Empty;
            var tokensUsed = response.Usage.InputTokens + response.Usage.OutputTokens;

            return new LLMResponse
            {
                Content = content,
                Provider = ProviderName,
                Model = model,
                TokensUsed = tokensUsed,
                Metadata = new Dictionary<string, object>
                {
                    { "StopReason", response.StopReason ?? "unknown" }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Anthropic completion");
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

        if (_client == null)
        {
            throw new InvalidOperationException("Anthropic client is not initialized");
        }

        try
        {
            var model = request.Model ?? _options.DefaultModel;
            var maxTokens = request.MaxTokens ?? _options.MaxTokens;

            var messages = new List<Message>();

            if (request.Messages != null && request.Messages.Count > 0)
            {
                foreach (var message in request.Messages)
                {
                    if (message.Role.ToLower() != "system")
                    {
                        messages.Add(new Message
                        {
                            Role = message.Role.ToLower() == "assistant" ? "assistant" : "user",
                            Content = message.Content
                        });
                    }
                }
            }
            else
            {
                messages.Add(new Message { Role = "user", Content = request.Prompt });
            }

            var messageRequest = new MessagesRequest
            {
                Model = model,
                MaxTokens = maxTokens,
                Messages = messages
            };

            var response = await _client.Messages.CreateAsync(messageRequest, cancellationToken);

            var content = response.Content.FirstOrDefault()?.Text ?? string.Empty;
            var tokensUsed = response.Usage.InputTokens + response.Usage.OutputTokens;

            return new LLMResponse
            {
                Content = content,
                Provider = ProviderName,
                Model = model,
                TokensUsed = tokensUsed,
                Metadata = new Dictionary<string, object>
                {
                    { "StopReason", response.StopReason ?? "unknown" }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Anthropic chat completion");
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

        return _client != null && !string.IsNullOrEmpty(_options.ApiKey);
    }
}

