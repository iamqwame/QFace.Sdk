using QFace.Sdk.RabbitMq.Services;

namespace QimErp.Shared.Common.Services.Notifications;

public interface IMessagingService
{

}

public class MessagingService(IRabbitMqPublisher publisher, ILogger<MessagingService> logger)
    : IMessagingService
{
    private const string ExchangeName = "umat.core.notify.prod_exchange";

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
                    Message = $"Hello {username}, you have successfully logged into your UMaT account."
                };

                logger.LogInformation("📤 [UnifiedMessage] Publishing login SMS notification for {PhoneNumber}...", phoneNumber);
                await publisher.PublishAsync(smsMessage, ExchangeName);
                logger.LogInformation("📤 [UnifiedMessage] Event published successfully: {Payload}", smsMessage.Serialize());
            }

            if (!string.IsNullOrEmpty(email))
            {
                var emailMessage = new UnifiedMessageModel
                {
                    MessageType = "simple_email",
                    ToEmail = email,
                    Subject = "Login Notification",
                    Body = $"Hello {username}, you have successfully logged into your UMaT account."
                };

                logger.LogInformation("📤 [UnifiedMessage] Publishing login email notification for {Email}...", email);
                await publisher.PublishAsync(emailMessage, ExchangeName);
                logger.LogInformation("📤 [UnifiedMessage] Event published successfully: {Payload}", emailMessage.Serialize());
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
                    Message = $"Hello {username}, your UMaT account has been created successfully."
                };

                logger.LogInformation("📤 [UnifiedMessage] Publishing registration SMS notification for {PhoneNumber}...", phoneNumber);
                await publisher.PublishAsync(smsMessage, ExchangeName);
                logger.LogInformation("📤 [UnifiedMessage] Event published successfully: {Payload}", smsMessage.Serialize());
            }

            if (!string.IsNullOrEmpty(email))
            {
                var emailMessage = new UnifiedMessageModel
                {
                    MessageType = "templated_email",
                    ToEmail = email,
                    Subject = "Registration Successful",
                    Template = "registration-confirmation",
                    Replacements = new Dictionary<string, string>
                    {
                        { "UserName", username },
                        { "CurrentYear", DateTime.Now.Year.ToString() }
                    }
                };

                logger.LogInformation("📤 [UnifiedMessage] Publishing registration email notification for {Email}...", email);
                await publisher.PublishAsync(emailMessage, ExchangeName);
                logger.LogInformation("📤 [UnifiedMessage] Event published successfully: {Payload}", emailMessage.Serialize());
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending registration notification for user {Username}", username);
            throw;
        }
    }

    public async Task SendPasswordResetEmailAsync(string email, string token)
    {
        try
        {
            var message = new UnifiedMessageModel
            {
                MessageType = "templated_email",
                ToEmail = email,
                Subject = "Password Reset Request",
                Template = "password-reset",
                Replacements = new Dictionary<string, string>
                {
                    { "ResetToken", token },
                    { "ExpiryHours", "24" }
                }
            };

            logger.LogInformation("📤 [UnifiedMessage] Publishing password reset email for {Email}...", email);
            await publisher.PublishAsync(message, ExchangeName);
            logger.LogInformation("📤 [UnifiedMessage] Event published successfully: {Payload}", message.Serialize());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending password reset email to {Email}", email);
            throw;
        }
    }

    public async Task SendEmailVerificationAsync(string email, string token)
    {
        try
        {
            var message = new UnifiedMessageModel
            {
                MessageType = "templated_email",
                ToEmail = email,
                Subject = "Email Verification",
                Template = "email-verification",
                Replacements = new Dictionary<string, string>
                {
                    { "VerificationToken", token },
                    { "ExpiryHours", "24" }
                }
            };

            logger.LogInformation("📤 [UnifiedMessage] Publishing email verification message for {Email}...", email);
            await publisher.PublishAsync(message, ExchangeName);
            logger.LogInformation("📤 [UnifiedMessage] Event published successfully: {Payload}", message.Serialize());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending email verification message to {Email}", email);
            throw;
        }
    }
} 