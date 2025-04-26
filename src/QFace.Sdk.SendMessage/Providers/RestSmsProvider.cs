using Microsoft.Extensions.Options;
using System.Text;

namespace QFace.Sdk.SendMessage.Providers;

/// <summary>
/// Simple SMS provider implementation - direct mapping to SMS API
/// </summary>
/// <summary>
/// Simple SMS provider implementation - direct mapping to SMS API
/// </summary>
public class RestSmsProvider: ISmsProvider
{
    private readonly ILogger<RestSmsProvider> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _endpoint;
    private readonly string _apiKey;
    private readonly string _sender;

    public RestSmsProvider(IOptions<MessageConfig> options, ILogger<RestSmsProvider> logger)
    {
        _logger = logger;
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);

        // Get config values
        var config = options.Value;
        _endpoint = config.SMS.Endpoint;
        _apiKey = config.SMS.ApiKey;
        _sender = config.SMS.Sender;
    }

    /// <summary>
    /// Sends an SMS message to one or more recipients
    /// </summary>
    public async Task<(bool Success, string ResponseContent)> SendSmsAsync(List<string> phoneNumbers, string message)
{
    try
    {
        _logger.LogInformation("📱 Sending SMS to {PhoneNumbers}", string.Join(", ", phoneNumbers));

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, _endpoint);
        requestMessage.Headers.TryAddWithoutValidation("Host", "api.smsonlinegh.com");
        requestMessage.Headers.TryAddWithoutValidation("Content-Type", "application/json");
        requestMessage.Headers.TryAddWithoutValidation("Accept", "application/json");
        requestMessage.Headers.TryAddWithoutValidation("Authorization", $"key {_apiKey}");

        var payload = new Dictionary<string, object>
        {
            { "text", message },
            { "type", 0 },
            { "sender", _sender },
            { "destinations", phoneNumbers }
        };

        string jsonContent = JsonSerializer.Serialize(payload);
        requestMessage.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(requestMessage);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("⚠️ HTTP request failed. Status: {StatusCode}. Response: {ResponseContent}",
                response.StatusCode, responseContent);
            return (false, responseContent);
        }

        // Now parse the responseContent to check delivery
        try
        {
            var parsed = JsonSerializer.Deserialize<SmsApiResponse>(responseContent);

            if (parsed?.Data?.Delivery == true)
            {
                _logger.LogInformation("✅ SMS delivery confirmed! Batch: {BatchId}", parsed.Data.Batch);
                return (true, responseContent);
            }
            else
            {
                _logger.LogWarning("⚠️ SMS delivery failed. Reason: {FirstReason}", 
                    parsed?.Data?.Destinations?.FirstOrDefault()?.Status?.Label ?? "Unknown");
                return (false, responseContent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to parse SMS API response.");
            return (false, responseContent);
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "❌ Exception sending SMS: {Message}", ex.Message);
        return (false, ex.Message);
    }
}
    public class SmsApiResponse
    {
        public SmsHandshake Handshake { get; set; }
        public SmsData Data { get; set; }
    }

    public class SmsHandshake
    {
        public int Id { get; set; }
        public string Label { get; set; }
    }

    public class SmsData
    {
        public string Batch { get; set; }
        public string Category { get; set; }
        public bool Delivery { get; set; }
        public List<SmsDestination> Destinations { get; set; }
    }

    public class SmsDestination
    {
        public string To { get; set; }
        public string Id { get; set; }
        public SmsStatus Status { get; set; }
    }

    public class SmsStatus
    {
        public int Id { get; set; }
        public string Label { get; set; }
    }



}