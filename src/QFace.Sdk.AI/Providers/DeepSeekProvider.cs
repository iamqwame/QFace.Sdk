using System.ClientModel;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using QFace.Sdk.AI.Models;
using QFace.Sdk.AI.Services;

namespace QFace.Sdk.AI.Providers;

/// <summary>
/// DeepSeek LLM provider — uses the OpenAI-compatible chat completions API
/// </summary>
public class DeepSeekProvider : ILLMProvider
{
    private readonly IAIOptionsProvider _optionsProvider;
    private readonly ILogger<DeepSeekProvider> _logger;

    /// <summary>
    /// Provider name
    /// </summary>
    public string ProviderName => "DeepSeek";

    /// <summary>
    /// Initializes a new instance of DeepSeekProvider
    /// </summary>
    public DeepSeekProvider(IAIOptionsProvider optionsProvider, ILogger<DeepSeekProvider> logger)
    {
        _optionsProvider = optionsProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> InitializeAsync()
    {
        var options = (await _optionsProvider.GetOptionsAsync()).DeepSeek;
        if (string.IsNullOrEmpty(options.ApiKey))
        {
            _logger.LogWarning("DeepSeek API key is not configured");
            return false;
        }

        return true;
    }

    /// <inheritdoc />
    public Task<LLMResponse> GenerateCompletionAsync(LLMRequest request, CancellationToken cancellationToken = default)
        => GenerateChatCompletionAsync(request, cancellationToken);

    /// <inheritdoc />
    public async Task<LLMResponse> GenerateChatCompletionAsync(LLMRequest request, CancellationToken cancellationToken = default)
    {
        var providerOptions = (await _optionsProvider.GetOptionsAsync(cancellationToken)).DeepSeek;
        if (string.IsNullOrEmpty(providerOptions.ApiKey))
        {
            throw new InvalidOperationException("DeepSeek provider is not initialized");
        }

        try
        {
            var model = request.Model ?? providerOptions.DefaultModel;
            var client = CreateClient(providerOptions);
            var chatClient = client.GetChatClient(model);
            var messages = BuildChatMessages(request);
            var options = BuildCompletionOptions(request, providerOptions);

            var completion = await CompleteWithRateLimitRetryAsync(
                () => chatClient.CompleteChatAsync(messages, options, cancellationToken),
                cancellationToken);

            var content = completion.Value.Content.Count > 0
                ? completion.Value.Content[0].Text ?? string.Empty
                : string.Empty;

            var usage = completion.Value.Usage;
            return new LLMResponse
            {
                Content = content,
                Provider = ProviderName,
                Model = model,
                TokensUsed = (usage?.InputTokenCount ?? 0) + (usage?.OutputTokenCount ?? 0),
                Metadata = new Dictionary<string, object>
                {
                    { "FinishReason", completion.Value.FinishReason.ToString() }
                }
            };
        }
        catch (ClientResultException ex)
        {
            _logger.LogError(ex, "Error generating DeepSeek chat completion");
            throw new InvalidOperationException(
                $"AI generation is currently unavailable (DeepSeek API returned {ex.Status}). " +
                "Contact your administrator to check the configured API key.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating DeepSeek chat completion");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> IsAvailableAsync()
    {
        var options = (await _optionsProvider.GetOptionsAsync()).DeepSeek;
        return !string.IsNullOrEmpty(options.ApiKey);
    }

    private static OpenAIClient CreateClient(DeepSeekOptions options)
    {
        var clientOptions = new OpenAIClientOptions();
        var baseUrl = (string.IsNullOrWhiteSpace(options.BaseUrl) ? "https://api.deepseek.com/v1" : options.BaseUrl).TrimEnd('/');
        clientOptions.Endpoint = new Uri(baseUrl + "/");
        return new OpenAIClient(new ApiKeyCredential(options.ApiKey), clientOptions);
    }

    private static List<ChatMessage> BuildChatMessages(LLMRequest request)
    {
        var messages = new List<ChatMessage>();

        if (request.Messages is { Count: > 0 })
        {
            foreach (var message in request.Messages)
            {
                messages.Add(message.Role.ToLowerInvariant() switch
                {
                    "system" => new SystemChatMessage(message.Content),
                    "assistant" => new AssistantChatMessage(message.Content),
                    _ => new UserChatMessage(message.Content)
                });
            }

            if (!string.IsNullOrWhiteSpace(request.Prompt))
            {
                messages.Add(new UserChatMessage(request.Prompt));
            }
        }
        else if (!string.IsNullOrWhiteSpace(request.Prompt))
        {
            messages.Add(new UserChatMessage(request.Prompt));
        }
        else
        {
            throw new ArgumentException("Either Prompt or Messages must be provided");
        }

        return messages;
    }

    private static ChatCompletionOptions BuildCompletionOptions(LLMRequest request, DeepSeekOptions providerOptions)
    {
        var options = new ChatCompletionOptions();
        var maxTokens = request.MaxTokens ?? providerOptions.MaxTokens;
        if (maxTokens > 0)
        {
            options.MaxOutputTokenCount = maxTokens;
        }

        var temperature = request.Temperature ?? providerOptions.Temperature;
        options.Temperature = (float)temperature;
        return options;
    }

    private async Task<ClientResult<ChatCompletion>> CompleteWithRateLimitRetryAsync(
        Func<Task<ClientResult<ChatCompletion>>> complete,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                return await complete();
            }
            catch (ClientResultException ex) when (ex.Status == 429 && attempt < 3)
            {
                var delayMs = attempt * 5000;
                _logger.LogWarning(
                    "DeepSeek rate limit (429) on attempt {Attempt}/3, retrying in {DelayMs}ms",
                    attempt,
                    delayMs);
                await Task.Delay(delayMs, cancellationToken);
            }
        }

        return await complete();
    }
}
