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

        // Always log the raw API response for debugging
        _logger.LogInformation("📡 SMS API Response: {ResponseContent}", responseContent);

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

            // Check if handshake was successful first
            if (parsed?.Handshake?.Label != "HSHK_OK")
            {
                _logger.LogWarning("⚠️ SMS API handshake failed. Label: {Label}", parsed?.Handshake?.Label ?? "Unknown");
                return (false, responseContent);
            }

            // Check delivery status based on destination status codes
            var firstDestination = parsed?.Data?.Destinations?.FirstOrDefault();
            if (firstDestination?.Status != null)
            {
                var statusId = firstDestination.Status.Id;
                var statusLabel = firstDestination.Status.Label;

                // Success status codes: DS_SUBMIT_ENROUTE (2105) indicates message is successfully queued
                if (statusId == 2105 || statusLabel == "DS_SUBMIT_ENROUTE")
                {
                    _logger.LogInformation("✅ SMS successfully queued for delivery! Batch: {BatchId}, Status: {Status}", 
                        parsed.Data.Batch, statusLabel);
                    return (true, responseContent);
                }
                else
                {
                    _logger.LogWarning("⚠️ SMS delivery failed. Status ID: {StatusId}, Label: {StatusLabel}", 
                        statusId, statusLabel);
                    return (false, responseContent);
                }
            }
            else
            {
                _logger.LogWarning("⚠️ SMS delivery status unknown. No destination status found.");
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