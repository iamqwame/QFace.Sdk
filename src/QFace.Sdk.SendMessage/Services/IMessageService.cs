namespace QFace.Sdk.SendMessage.Services;

/// <summary>
/// Interface for unified message service operations (email and SMS)
/// </summary>
public interface IMessageService
{
    /// <summary>
    /// Sends an email to one or more recipients
    /// </summary>
    Task<bool> SendEmailAsync(List<string> toEmail, string subject, string body);

    /// <summary>
    /// Sends an email using a template with placeholder values
    /// </summary>
    Task<bool> SendEmailWithTemplateAsync(List<string> toEmail, string subject, string template, 
        Dictionary<string, string> replacements);

    /// <summary>
    /// Sends an SMS to one or more recipients
    /// </summary>
    Task<bool> SendSmsAsync(List<string> toPhoneNumbers, string message);

    /// <summary>
    /// Sends an SMS using a template with placeholder values
    /// </summary>
    Task<bool> SendSmsWithTemplateAsync(List<string> toPhoneNumbers, string template, 
        Dictionary<string, string> replacements);

    /// <summary>
    /// Sends both an email and SMS to recipients
    /// </summary>
    Task<bool> SendBothAsync(List<string> toEmail, List<string> toPhoneNumbers, 
        string subject, string body);

    /// <summary>
    /// Processes a message command to determine the appropriate sending method
    /// </summary>
    Task<bool> ProcessMessageCommandAsync(SendMessageCommand command);
}