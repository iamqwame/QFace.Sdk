using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using QFace.Sdk.AI.Models;

namespace QFace.Sdk.AI.Providers;

/// <summary>
/// Google Gemini LLM provider implementation using REST API
/// </summary>
public class GoogleGeminiProvider : ILLMProvider
{
    private readonly GoogleGeminiOptions _options;
    private readonly ILogger<GoogleGeminiProvider> _logger;
    private readonly HttpClient _httpClient;
    private bool _initialized;

    /// <summary>
    /// Provider name
    /// </summary>
    public string ProviderName => "GoogleGemini";

    /// <summary>
    /// Initializes a new instance of GoogleGeminiProvider
    /// </summary>
    public GoogleGeminiProvider(IOptions<AIOptions> aiOptions, ILogger<GoogleGeminiProvider> logger, HttpClient httpClient)
    {
        _options = aiOptions.Value.GoogleGemini;
        _logger = logger;
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public Task<bool> InitializeAsync()
    {
        if (string.IsNullOrEmpty(_options.ApiKey))
        {
            _logger.LogWarning("Google Gemini API key is not configured");
            return Task.FromResult(false);
        }

        _initialized = true;
        _logger.LogInformation("Google Gemini provider initialized successfully");
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public async Task<LLMResponse> GenerateCompletionAsync(LLMRequest request, CancellationToken cancellationToken = default)
    {
        if (!_initialized)
        {
            await InitializeAsync();
        }

        var model = request.Model ?? _options.DefaultModel;
        var maxTokens = request.MaxTokens ?? _options.MaxTokens;
        var temperature = request.Temperature ?? 0.7;

        var url = $"{_options.BaseUrl}/models/{model}:generateContent?key={_options.ApiKey}";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = request.Prompt }
                    }
                }
            },
            generationConfig = new
            {
                temperature = temperature,
                maxOutputTokens = maxTokens
            }
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseData = await response.Content.ReadFromJsonAsync<GeminiResponse>(cancellationToken: cancellationToken);

            if (responseData?.Candidates == null || responseData.Candidates.Length == 0)
            {
                throw new InvalidOperationException("No response candidates returned from Gemini API");
            }

            var content = responseData.Candidates[0].Content?.Parts?[0]?.Text ?? string.Empty;
            var tokensUsed = responseData.UsageMetadata?.TotalTokenCount ?? 0;

            return new LLMResponse
            {
                Content = content,
                Provider = ProviderName,
                Model = model,
                TokensUsed = tokensUsed,
                Metadata = new Dictionary<string, object>
                {
                    ["finishReason"] = responseData.Candidates[0].FinishReason ?? "unknown"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Google Gemini API");
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

        var model = request.Model ?? _options.DefaultModel;
        var maxTokens = request.MaxTokens ?? _options.MaxTokens;
        var temperature = request.Temperature ?? 0.7;

        var url = $"{_options.BaseUrl}/models/{model}:generateContent?key={_options.ApiKey}";

        // Build contents from messages or prompt
        var contents = new List<object>();

        if (request.Messages != null && request.Messages.Count > 0)
        {
            foreach (var message in request.Messages)
            {
                // Google Gemini uses "user" and "model" roles (not "assistant")
                var role = message.Role.ToLower() switch
                {
                    "assistant" => "model",
                    "user" => "user",
                    "system" => "user", // System messages are treated as user messages in Gemini
                    _ => "user"
                };

                contents.Add(new
                {
                    role = role,
                    parts = new[]
                    {
                        new { text = message.Content }
                    }
                });
            }
        }
        else if (!string.IsNullOrEmpty(request.Prompt))
        {
            contents.Add(new
            {
                parts = new[]
                {
                    new { text = request.Prompt }
                }
            });
        }
        else
        {
            throw new ArgumentException("Either Prompt or Messages must be provided");
        }

        var requestBody = new
        {
            contents = contents,
            generationConfig = new
            {
                temperature = temperature,
                maxOutputTokens = maxTokens
            }
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseData = await response.Content.ReadFromJsonAsync<GeminiResponse>(cancellationToken: cancellationToken);

            if (responseData?.Candidates == null || responseData.Candidates.Length == 0)
            {
                throw new InvalidOperationException("No response candidates returned from Gemini API");
            }

            var content = responseData.Candidates[0].Content?.Parts?[0]?.Text ?? string.Empty;
            var tokensUsed = responseData.UsageMetadata?.TotalTokenCount ?? 0;

            return new LLMResponse
            {
                Content = content,
                Provider = ProviderName,
                Model = model,
                TokensUsed = tokensUsed,
                Metadata = new Dictionary<string, object>
                {
                    ["finishReason"] = responseData.Candidates[0].FinishReason ?? "unknown"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Google Gemini API");
            throw;
        }
    }

    /// <inheritdoc />
    public Task<bool> IsAvailableAsync()
    {
        return Task.FromResult(_initialized && !string.IsNullOrEmpty(_options.ApiKey));
    }

    // Internal classes for Gemini API response
    private class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public GeminiCandidate[]? Candidates { get; set; }

        [JsonPropertyName("usageMetadata")]
        public GeminiUsageMetadata? UsageMetadata { get; set; }
    }

    private class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiContent? Content { get; set; }

        [JsonPropertyName("finishReason")]
        public string? FinishReason { get; set; }
    }

    private class GeminiContent
    {
        [JsonPropertyName("parts")]
        public GeminiPart[]? Parts { get; set; }
    }

    private class GeminiPart
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    private class GeminiUsageMetadata
    {
        [JsonPropertyName("promptTokenCount")]
        public int PromptTokenCount { get; set; }

        [JsonPropertyName("candidatesTokenCount")]
        public int CandidatesTokenCount { get; set; }

        [JsonPropertyName("totalTokenCount")]
        public int TotalTokenCount { get; set; }
    }
}
