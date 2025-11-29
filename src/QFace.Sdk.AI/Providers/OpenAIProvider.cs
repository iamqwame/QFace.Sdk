using OpenAI;
using OpenAI.Chat;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using QFace.Sdk.AI.Models;

namespace QFace.Sdk.AI.Providers;

/// <summary>
/// OpenAI LLM provider implementation
/// </summary>
public class OpenAIProvider : ILLMProvider
{
    private readonly OpenAIOptions _options;
    private readonly ILogger<OpenAIProvider> _logger;
    private OpenAIClient? _client;
    private bool _initialized;

    /// <summary>
    /// Provider name
    /// </summary>
    public string ProviderName => "OpenAI";

    /// <summary>
    /// Initializes a new instance of OpenAIProvider
    /// </summary>
    public OpenAIProvider(IOptions<AIOptions> aiOptions, ILogger<OpenAIProvider> logger)
    {
        _options = aiOptions.Value.OpenAI;
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
                _logger.LogWarning("OpenAI API key is not configured");
                return false;
            }

            _client = new OpenAIClient(_options.ApiKey);
            _initialized = true;
            _logger.LogInformation("OpenAI provider initialized successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize OpenAI provider");
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
            throw new InvalidOperationException("OpenAI client is not initialized");
        }

        try
        {
            var model = request.Model ?? _options.DefaultModel;
            var maxTokens = request.MaxTokens ?? _options.MaxTokens;
            var temperature = request.Temperature ?? _options.Temperature;

            var chatClient = new ChatClient(_client, model);
            var chatMessages = new List<ChatMessage>
            {
                new UserChatMessage(request.Prompt)
            };

            var chatCompletionOptions = new ChatCompletionOptions
            {
                MaxTokens = maxTokens,
                Temperature = (float)temperature
            };

            var response = await chatClient.CompleteChatAsync(chatMessages, chatCompletionOptions, cancellationToken);

            return new LLMResponse
            {
                Content = response.Value.Content[0].Text,
                Provider = ProviderName,
                Model = model,
                TokensUsed = response.Value.Usage.TotalTokens,
                Metadata = new Dictionary<string, object>
                {
                    { "FinishReason", response.Value.FinishReason.ToString() }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating OpenAI completion");
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
            throw new InvalidOperationException("OpenAI client is not initialized");
        }

        try
        {
            var model = request.Model ?? _options.DefaultModel;
            var maxTokens = request.MaxTokens ?? _options.MaxTokens;
            var temperature = request.Temperature ?? _options.Temperature;

            var chatClient = new ChatClient(_client, model);
            var chatMessages = new List<ChatMessage>();

            if (request.Messages != null && request.Messages.Count > 0)
            {
                foreach (var message in request.Messages)
                {
                    switch (message.Role.ToLower())
                    {
                        case "system":
                            chatMessages.Add(new SystemChatMessage(message.Content));
                            break;
                        case "user":
                            chatMessages.Add(new UserChatMessage(message.Content));
                            break;
                        case "assistant":
                            chatMessages.Add(new AssistantChatMessage(message.Content));
                            break;
                    }
                }
            }
            else
            {
                chatMessages.Add(new UserChatMessage(request.Prompt));
            }

            var chatCompletionOptions = new ChatCompletionOptions
            {
                MaxTokens = maxTokens,
                Temperature = (float)temperature
            };

            var response = await chatClient.CompleteChatAsync(chatMessages, chatCompletionOptions, cancellationToken);

            return new LLMResponse
            {
                Content = response.Value.Content[0].Text,
                Provider = ProviderName,
                Model = model,
                TokensUsed = response.Value.Usage.TotalTokens,
                Metadata = new Dictionary<string, object>
                {
                    { "FinishReason", response.Value.FinishReason.ToString() }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating OpenAI chat completion");
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

