using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using QFace.Sdk.AI.Models;
using QFace.Sdk.AI.Services;

namespace QFace.Sdk.AI.Providers;

/// <summary>
/// Google Gemini LLM provider implementation using REST API
/// </summary>
public class GoogleGeminiProvider : ILLMProvider
{
    private readonly IAIOptionsProvider _optionsProvider;
    private readonly ILogger<GoogleGeminiProvider> _logger;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Provider name
    /// </summary>
    public string ProviderName => "GoogleGemini";

    /// <summary>
    /// Initializes a new instance of GoogleGeminiProvider
    /// </summary>
    public GoogleGeminiProvider(IAIOptionsProvider optionsProvider, ILogger<GoogleGeminiProvider> logger, HttpClient httpClient)
    {
        _optionsProvider = optionsProvider;
        _logger = logger;
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<bool> InitializeAsync()
    {
        var options = (await _optionsProvider.GetOptionsAsync()).GoogleGemini;
        if (string.IsNullOrEmpty(options.ApiKey))
        {
            _logger.LogWarning("Google Gemini API key is not configured");
            return false;
        }

        return true;
    }

    /// <inheritdoc />
    public async Task<LLMResponse> GenerateCompletionAsync(LLMRequest request, CancellationToken cancellationToken = default)
    {
        var providerOptions = (await _optionsProvider.GetOptionsAsync(cancellationToken)).GoogleGemini;

        var model = request.Model ?? providerOptions.DefaultModel;
        var maxTokens = request.MaxTokens ?? providerOptions.MaxTokens;
        var temperature = request.Temperature ?? 0.7;

        var url = $"{providerOptions.BaseUrl}/models/{model}:generateContent?key={providerOptions.ApiKey}";

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
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error calling Google Gemini API");
            throw new InvalidOperationException(
                $"AI generation is currently unavailable (Gemini API returned {(int?)ex.StatusCode ?? 0}). " +
                "Contact your administrator to check the configured API key.", ex);
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
        var providerOptions = (await _optionsProvider.GetOptionsAsync(cancellationToken)).GoogleGemini;

        var model = request.Model ?? providerOptions.DefaultModel;
        var maxTokens = request.MaxTokens ?? providerOptions.MaxTokens;
        var temperature = request.Temperature ?? 0.7;

        var url = $"{providerOptions.BaseUrl}/models/{model}:generateContent?key={providerOptions.ApiKey}";

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
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error calling Google Gemini API");
            throw new InvalidOperationException(
                $"AI generation is currently unavailable (Gemini API returned {(int?)ex.StatusCode ?? 0}). " +
                "Contact your administrator to check the configured API key.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Google Gemini API");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> IsAvailableAsync()
    {
        var options = (await _optionsProvider.GetOptionsAsync()).GoogleGemini;
        return !string.IsNullOrEmpty(options.ApiKey);
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
