namespace QFace.Sdk.SendMessage.Providers;

/// <summary>
/// Interface for email providers
/// </summary>
public interface IEmailProvider
{
    /// <summary>
    /// Provider name
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Initializes the provider
    /// </summary>
    Task<bool> Initialize();

    /// <summary>
    /// Sends an email to one or more recipients
    /// </summary>
    /// <param name="toEmail">List of recipient email addresses</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body (HTML format)</param>
    /// <returns>True if sending was successful, otherwise false</returns>
    Task<bool> SendEmailAsync(List<string> toEmail, string subject, string body);
}