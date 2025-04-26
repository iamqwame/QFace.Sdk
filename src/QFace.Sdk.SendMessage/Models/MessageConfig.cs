namespace QFace.Sdk.SendMessage.Models;

/// <summary>
/// Configuration for email settings
/// </summary>
public class EmailConfig
{
    public string SmtpServer { get; set; }
    public int SmtpPort { get; set; } = 587;
    public string SmtpUser { get; set; }
    public string SmtpPassword { get; set; }
    public string FromEmail { get; set; }
    public string FromName { get; set; }
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
    public string ApiKey { get; set; }

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