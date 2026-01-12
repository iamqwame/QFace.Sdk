using System.Text.Json.Serialization;

namespace QimErp.Shared.Common.Events;

public class UnifiedMessageModel
{
    // Required for all message types
    [JsonRequired]
    public string MessageType { get; set; }
    
    // Email properties
    public string ToEmail { get; set; }
    public List<string> ToEmails { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    
    // Template properties
    public string Template { get; set; }
    public Dictionary<string, string> Replacements { get; set; }
    
    // SMS properties
    public string PhoneNumber { get; set; }
    public List<string> PhoneNumbers { get; set; }
    public string Message { get; set; }
    
    // Combined properties
    public string Email { get; set; }
    
    // Metadata properties that might be useful
    public string MessageId { get; set; }
    public string CorrelationId { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
    
    // Timestamp for tracking
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}