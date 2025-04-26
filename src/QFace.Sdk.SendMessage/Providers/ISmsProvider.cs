namespace QFace.Sdk.SendMessage.Providers;

/// <summary>
/// Interface for SMS providers
/// </summary>
public interface ISmsProvider
{
    /// <summary>
    /// Sends an SMS message to one or more recipients
    /// </summary>
    /// <param name="phoneNumbers">List of recipient phone numbers</param>
    /// <param name="message">SMS message content</param>
    /// <returns>True if sending was successful, otherwise false</returns>
    Task<(bool Success, string ResponseContent)> SendSmsAsync(List<string> phoneNumbers, string message);
}