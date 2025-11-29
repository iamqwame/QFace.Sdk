using Microsoft.Extensions.AI;
using OpenAI;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using QFace.Sdk.AI.Models;

namespace QFace.Sdk.AI.Providers;

/// <summary>
/// OpenAI LLM provider implementation using Microsoft.Extensions.AI
/// </summary>
public class OpenAIProvider : ILLMProvider
{
    private readonly OpenAIOptions _options;
    private readonly ILogger<OpenAIProvider> _logger;
    private IChatClient? _chatClient;
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
    public Task<bool> InitializeAsync()
    {
        if (_initialized)
        {
            return Task.FromResult(true);
        }

        try
        {
            if (string.IsNullOrEmpty(_options.ApiKey))
            {
                _logger.LogWarning("OpenAI API key is not configured");
                return Task.FromResult(false);
            }

            var client = new OpenAIClient(_options.ApiKey);
            var model = _options.DefaultModel;
            _chatClient = client.AsChatClient(modelId: model);
            _initialized = true;
            _logger.LogInformation("OpenAI provider initialized successfully with model {Model}", model);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize OpenAI provider");
            return Task.FromResult(false);
        }
    }

    /// <inheritdoc />
    public async Task<LLMResponse> GenerateCompletionAsync(LLMRequest request, CancellationToken cancellationToken = default)
    {
        if (!_initialized)
        {
            await InitializeAsync();
        }

        if (_chatClient == null)
        {
            throw new InvalidOperationException("OpenAI chat client is not initialized");
        }

        try
        {
            var model = request.Model ?? _options.DefaultModel;
            var chatClient = await GetOrCreateChatClientAsync(model);

            // Use the prompt directly for completion
            var response = await chatClient.CompleteAsync(request.Prompt, cancellationToken: cancellationToken);

            // Extract content from ChatMessage - it might be Text, Content, or ToString()
            var content = response.Message.ToString();
            
            return new LLMResponse
            {
                Content = content,
                Provider = ProviderName,
                Model = model,
                TokensUsed = (response.Usage?.InputTokenCount ?? 0) + (response.Usage?.OutputTokenCount ?? 0),
                Metadata = new Dictionary<string, object>
                {
                    { "FinishReason", response.FinishReason?.ToString() ?? "unknown" }
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

        if (_chatClient == null)
        {
            throw new InvalidOperationException("OpenAI chat client is not initialized");
        }

        try
        {
            var model = request.Model ?? _options.DefaultModel;
            var chatClient = await GetOrCreateChatClientAsync(model);

            // Build the prompt from messages or use the prompt directly
            string prompt;
            if (request.Messages != null && request.Messages.Count > 0)
            {
                var messageParts = request.Messages.Select(m => $"{m.Role}: {m.Content}");
                prompt = string.Join("\n", messageParts);
                if (!string.IsNullOrEmpty(request.Prompt))
                {
                    prompt += $"\nuser: {request.Prompt}";
                }
            }
            else
            {
                prompt = request.Prompt;
            }

            var response = await chatClient.CompleteAsync(prompt, cancellationToken: cancellationToken);

            // Extract content from ChatMessage - it might be Text, Content, or ToString()
            var content = response.Message.ToString();
            
            return new LLMResponse
            {
                Content = content,
                Provider = ProviderName,
                Model = model,
                TokensUsed = (response.Usage?.InputTokenCount ?? 0) + (response.Usage?.OutputTokenCount ?? 0),
                Metadata = new Dictionary<string, object>
                {
                    { "FinishReason", response.FinishReason?.ToString() ?? "unknown" }
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

        return _chatClient != null && !string.IsNullOrEmpty(_options.ApiKey);
    }

    /// <summary>
    /// Gets or creates a chat client for the specified model
    /// </summary>
    private Task<IChatClient> GetOrCreateChatClientAsync(string model)
    {
        // If model changed or client not initialized, create new client
        if (_chatClient == null || model != _options.DefaultModel)
        {
            var client = new OpenAIClient(_options.ApiKey);
            _chatClient = client.AsChatClient(modelId: model);
        }

        return Task.FromResult(_chatClient);
    }
}
