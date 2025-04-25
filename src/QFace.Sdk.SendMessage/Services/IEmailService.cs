namespace QFace.Sdk.SendMessage.Services;

/// <summary>
/// Interface for email service operations
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email to one or more recipients
    /// </summary>
    /// <param name="toEmail">List of recipient email addresses</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body (HTML format)</param>
    /// <returns>True if sending was successful, otherwise false</returns>
    Task<bool> SendEmailAsync(List<string> toEmail, string subject, string body);

    /// <summary>
    /// Sends an email using a template with placeholder values
    /// </summary>
    /// <param name="toEmail">List of recipient email addresses</param>
    /// <param name="subject">Email subject</param>
    /// <param name="template">HTML template with placeholders in {{PlaceholderName}} format</param>
    /// <param name="replacements">Dictionary of placeholder name and value pairs</param>
    /// <returns>True if sending was successful, otherwise false</returns>
    Task<bool> SendEmailWithTemplateAsync(List<string> toEmail, string subject, string template, Dictionary<string, string> replacements);
}