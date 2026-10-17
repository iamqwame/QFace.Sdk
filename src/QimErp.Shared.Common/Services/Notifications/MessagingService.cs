namespace QimErp.Shared.Common.Services.Notifications;

public interface IMessagingService
{
    Task SendLoginNotificationAsync(string username, string? phoneNumber, string? email);
    Task SendRegistrationNotificationAsync(string username, string? phoneNumber, string? email);
}

public class MessagingService(
    INotificationWorkflowStarter notificationStarter,
    ILogger<MessagingService> logger)
    : IMessagingService
{
    public async Task SendLoginNotificationAsync(string username, string? phoneNumber, string? email)
    {
        try
        {
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                var smsMessage = new UnifiedMessageModel
                {
                    MessageType = "sms",
                    PhoneNumber = phoneNumber,
                    Message = $"Hello {username}, you have successfully logged into your QimERP account."
                };

                logger.LogInformation("[Messaging] Queuing login SMS for {PhoneNumber}...", phoneNumber);
                await notificationStarter.SendAsync(smsMessage);
            }

            if (!string.IsNullOrEmpty(email))
            {
                logger.LogInformation("[Messaging] Queuing login email for {Email}...", email);
                await notificationStarter.SendEmailAsync(
                    email,
                    "Login Notification",
                    $"Hello {username}, you have successfully logged into your QimERP account.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending login notification for user {Username}", username);
            throw;
        }
    }

    public async Task SendRegistrationNotificationAsync(string username, string? phoneNumber, string? email)
    {
        try
        {
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                var smsMessage = new UnifiedMessageModel
                {
                    MessageType = "sms",
                    PhoneNumber = phoneNumber,
                    Message = $"Hello {username}, your QimERP account has been created successfully."
                };

                logger.LogInformation("[Messaging] Queuing registration SMS for {PhoneNumber}...", phoneNumber);
                await notificationStarter.SendAsync(smsMessage);
            }

            if (!string.IsNullOrEmpty(email))
            {
                logger.LogInformation("[Messaging] Queuing registration email for {Email}...", email);
                await notificationStarter.SendTemplatedEmailAsync(
                    email,
                    "Registration Successful",
                    "registration-confirmation",
                    new Dictionary<string, string>
                    {
                        { "UserName", username },
                        { "CurrentYear", DateTime.Now.Year.ToString() }
                    });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending registration notification for user {Username}", username);
            throw;
        }
    }
}
