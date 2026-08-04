using System.Net.Http.Json;
using System.Text.Json.Serialization;
using QFace.Sdk.AI.Services;

namespace QFace.Sdk.AI.Providers;

/// <summary>
/// Google Gemini embedding provider using REST embedContent API
/// </summary>
public class GoogleGeminiEmbeddingProvider : IEmbeddingProvider
{
    private readonly IAIOptionsProvider _optionsProvider;
    private readonly ILogger<GoogleGeminiEmbeddingProvider> _logger;
    private readonly HttpClient _httpClient;

    public string ProviderName => "GoogleGemini";

    public GoogleGeminiEmbeddingProvider(
        IAIOptionsProvider optionsProvider,
        ILogger<GoogleGeminiEmbeddingProvider> logger,
        HttpClient httpClient)
    {
        _optionsProvider = optionsProvider;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<bool> InitializeAsync()
    {
        var options = (await _optionsProvider.GetOptionsAsync()).GoogleGemini;
        if (string.IsNullOrEmpty(options.ApiKey))
        {
            _logger.LogWarning("Google Gemini API key is not configured for embeddings");
            return false;
        }

        return true;
    }

    public async Task<EmbeddingResponse> GenerateEmbeddingAsync(
        EmbeddingRequest request,
        CancellationToken cancellationToken = default)
    {
        var providerOptions = (await _optionsProvider.GetOptionsAsync(cancellationToken)).GoogleGemini;

        if (string.IsNullOrWhiteSpace(request.Text))
        {
            throw new ArgumentException("Text is required for embedding generation", nameof(request));
        }

        var model = request.Model ?? providerOptions.DefaultEmbeddingModel;
        var url = $"{providerOptions.BaseUrl}/models/{model}:embedContent?key={providerOptions.ApiKey}";

        var requestBody = new
        {
            content = new
            {
                parts = new[]
                {
                    new { text = request.Text }
                }
            }
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseData = await response.Content.ReadFromJsonAsync<GeminiEmbeddingResponse>(cancellationToken: cancellationToken);
            var values = responseData?.Embedding?.Values;

            if (values is null || values.Length == 0)
            {
                throw new InvalidOperationException("No embedding values returned from Gemini API");
            }

            return new EmbeddingResponse
            {
                Embedding = values,
                Provider = ProviderName,
                Model = model
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Google Gemini embedContent API");
            throw;
        }
    }

    public async Task<bool> IsAvailableAsync()
    {
        var options = (await _optionsProvider.GetOptionsAsync()).GoogleGemini;
        return !string.IsNullOrEmpty(options.ApiKey);
    }

    private class GeminiEmbeddingResponse
    {
        [JsonPropertyName("embedding")]
        public GeminiEmbedding? Embedding { get; set; }
    }

    private class GeminiEmbedding
    {
        [JsonPropertyName("values")]
        public float[]? Values { get; set; }
    }
}
