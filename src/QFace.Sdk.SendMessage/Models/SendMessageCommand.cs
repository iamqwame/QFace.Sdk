namespace QFace.Sdk.SendMessage.Models;

/// <summary>
/// Command for sending messages (email or SMS) via the actor system
/// </summary>
public class SendMessageCommand
{
    /// <summary>
    /// List of recipient email addresses (for email messages)
    /// </summary>
    public List<string> ToEmails { get; private set; } = new List<string>();

    /// <summary>
    /// List of recipient phone numbers (for SMS messages)
    /// </summary>
    public List<string> ToPhoneNumbers { get; private set; } = new List<string>();

    /// <summary>
    /// Message subject (for email)
    /// </summary>
    public string Subject { get; private set; } = string.Empty;

    /// <summary>
    /// Message body content (for non-templated messages)
    /// </summary>
    public string Body { get; private set; } = string.Empty;

    /// <summary>
    /// Message template name (for templated messages)
    /// </summary>
    public string Template { get; private set; } = string.Empty;

    /// <summary>
    /// Replacement values for template placeholders
    /// </summary>
    public Dictionary<string, string> Replacements { get; private set; } = new Dictionary<string, string>();

    /// <summary>
    /// Type of message to send (Email, SMS, or Both)
    /// </summary>
    public MessageType MessageType { get; private set; } = MessageType.Email;

    // Private constructor to enforce factory methods
    private SendMessageCommand()
    {
    }

    #region Email Factory Methods

    /// <summary>
    /// Creates an email-only command for a single recipient
    /// </summary>
    public static SendMessageCommand CreateEmailOnly(string toEmail, string subject, string body)
    {
        var command = new SendMessageCommand
        {
            Subject = subject,
            Body = body,
            MessageType = MessageType.Email
        };
        command.ToEmails.Add(toEmail);
        return command;
    }

    /// <summary>
    /// Creates an email-only command for multiple recipients
    /// </summary>
    public static SendMessageCommand CreateEmailOnly(List<string> toEmails, string subject, string body)
    {
        return new SendMessageCommand
        {
            ToEmails = toEmails,
            Subject = subject,
            Body = body,
            MessageType = MessageType.Email
        };
    }

    /// <summary>
    /// Creates a templated email-only command for a single recipient
    /// </summary>
    public static SendMessageCommand CreateEmailWithTemplateOnly(
        string toEmail,
        string subject,
        string template,
        Dictionary<string, string> replacements)
    {
        var command = new SendMessageCommand
        {
            Subject = subject,
            Template = template,
            Replacements = replacements,
            MessageType = MessageType.Email
        };
        command.ToEmails.Add(toEmail);
        return command;
    }

    /// <summary>
    /// Creates a templated email-only command for multiple recipients
    /// </summary>
    public static SendMessageCommand CreateEmailWithTemplateOnly(
        List<string> toEmails,
        string subject,
        string template,
        Dictionary<string, string> replacements)
    {
        return new SendMessageCommand
        {
            ToEmails = toEmails,
            Subject = subject,
            Template = template,
            Replacements = replacements,
            MessageType = MessageType.Email
        };
    }

    #endregion

    #region SMS Factory Methods

    /// <summary>
    /// Creates an SMS-only command for a single recipient
    /// </summary>
    public static SendMessageCommand CreateSMSOnly(string toPhoneNumber, string message)
    {
        var command = new SendMessageCommand
        {
            Body = message,
            MessageType = MessageType.SMS
        };
        command.ToPhoneNumbers.Add(toPhoneNumber);
        return command;
    }

    /// <summary>
    /// Creates an SMS-only command for multiple recipients
    /// </summary>
    public static SendMessageCommand CreateSMSOnly(List<string> toPhoneNumbers, string message)
    {
        return new SendMessageCommand
        {
            ToPhoneNumbers = toPhoneNumbers,
            Body = message,
            MessageType = MessageType.SMS
        };
    }

    /// <summary>
    /// Creates a templated SMS-only command for a single recipient
    /// </summary>
    public static SendMessageCommand CreateSMSWithTemplateOnly(
        string toPhoneNumber,
        string template,
        Dictionary<string, string> replacements)
    {
        var command = new SendMessageCommand
        {
            Template = template,
            Replacements = replacements,
            MessageType = MessageType.SMS
        };
        command.ToPhoneNumbers.Add(toPhoneNumber);
        return command;
    }

    /// <summary>
    /// Creates a templated SMS-only command for multiple recipients
    /// </summary>
    public static SendMessageCommand CreateSMSWithTemplateOnly(
        List<string> toPhoneNumbers,
        string template,
        Dictionary<string, string> replacements)
    {
        return new SendMessageCommand
        {
            ToPhoneNumbers = toPhoneNumbers,
            Template = template,
            Replacements = replacements,
            MessageType = MessageType.SMS
        };
    }

    #endregion

    #region Dual-channel Factory Methods

    /// <summary>
    /// Creates a message command for sending both email and SMS to a single recipient
    /// </summary>
    public static SendMessageCommand CreateBoth(
        string toEmail,
        string toPhoneNumber,
        string subject,
        string body)
    {
        var command = new SendMessageCommand
        {
            Subject = subject,
            Body = body,
            MessageType = MessageType.Both
        };
        command.ToEmails.Add(toEmail);
        command.ToPhoneNumbers.Add(toPhoneNumber);
        return command;
    }

    /// <summary>
    /// Creates a templated message command for sending both email and SMS to a single recipient
    /// </summary>
    public static SendMessageCommand CreateBothWithTemplate(
        string toEmail,
        string toPhoneNumber,
        string subject,
        string template,
        Dictionary<string, string> replacements)
    {
        var command = new SendMessageCommand
        {
            Subject = subject,
            Template = template,
            Replacements = replacements,
            MessageType = MessageType.Both
        };
        command.ToEmails.Add(toEmail);
        command.ToPhoneNumbers.Add(toPhoneNumber);
        return command;
    }

    #endregion
}