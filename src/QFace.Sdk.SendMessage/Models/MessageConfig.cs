namespace QFace.Sdk.SendMessage.Models;

/// <summary>
/// Configuration for email settings
/// </summary>
public class EmailConfig
{
    /// <summary>
    /// Email provider type: "SMTP" or "Graph"
    /// </summary>
    public string Provider { get; set; } = "SMTP";
    
    // SMTP Settings
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUser { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;

    // Microsoft Graph Settings
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string SendAsUser { get; set; } = string.Empty;

    // Common Settings
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public bool IsLocalHost { get; set; } = false;
}

/// <summary>
/// Configuration for SMS settings
/// </summary>
public class SmsConfig
{
    /// <summary>
    /// API endpoint URL
    /// </summary>
    public string Endpoint { get; set; } = "https://api.smsonlinegh.com/v5/sms/send";

    /// <summary>
    /// API key for authentication
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Sender ID for SMS messages
    /// </summary>
    public string Sender { get; set; } = "TEST";
}

/// <summary>
/// Overall messaging configuration
/// </summary>
public class MessageConfig
{
    public EmailConfig Email { get; set; } = new EmailConfig();
    public SmsConfig SMS { get; set; } = new SmsConfig();
}