using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace QFace.Sdk.AI.Providers;

/// <summary>
/// Google Gemini embedding provider using REST embedContent API
/// </summary>
public class GoogleGeminiEmbeddingProvider : IEmbeddingProvider
{
    private readonly GoogleGeminiOptions _options;
    private readonly ILogger<GoogleGeminiEmbeddingProvider> _logger;
    private readonly HttpClient _httpClient;
    private bool _initialized;

    public string ProviderName => "GoogleGemini";

    public GoogleGeminiEmbeddingProvider(
        IOptions<AIOptions> aiOptions,
        ILogger<GoogleGeminiEmbeddingProvider> logger,
        HttpClient httpClient)
    {
        _options = aiOptions.Value.GoogleGemini;
        _logger = logger;
        _httpClient = httpClient;
    }

    public Task<bool> InitializeAsync()
    {
        if (string.IsNullOrEmpty(_options.ApiKey))
        {
            _logger.LogWarning("Google Gemini API key is not configured for embeddings");
            return Task.FromResult(false);
        }

        _initialized = true;
        _logger.LogInformation("Google Gemini embedding provider initialized successfully");
        return Task.FromResult(true);
    }

    public async Task<EmbeddingResponse> GenerateEmbeddingAsync(
        EmbeddingRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_initialized)
        {
            await InitializeAsync();
        }

        if (string.IsNullOrWhiteSpace(request.Text))
        {
            throw new ArgumentException("Text is required for embedding generation", nameof(request));
        }

        var model = request.Model ?? _options.DefaultEmbeddingModel;
        var url = $"{_options.BaseUrl}/models/{model}:embedContent?key={_options.ApiKey}";

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

    public Task<bool> IsAvailableAsync()
    {
        return Task.FromResult(_initialized && !string.IsNullOrEmpty(_options.ApiKey));
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
