namespace QFace.Sdk.SendMessage.Actors;

/// <summary>
/// Actor for handling message sending operations (email and SMS)
/// </summary>
public class SendMessageActor : BaseActor
{
    private readonly ILogger<SendMessageActor> _logger;
    private readonly IEmailProvider _emailProvider;
    private readonly ISmsProvider _smsProvider;

    public SendMessageActor(
        ILogger<SendMessageActor> logger, 
        IEmailProvider emailProvider, 
        ISmsProvider smsProvider)
    {
        _logger = logger;
        _emailProvider = emailProvider;
        _smsProvider = smsProvider;
            
        // Register message handler
        Receive<SendMessageCommand>(HandleSendMessage);
    }

    /// <summary>
    /// Creates props for dependency injection
    /// </summary>
    public static Props Create(
        ILogger<SendMessageActor> logger,
        IEmailProvider emailProvider,
        ISmsProvider smsProvider)
    {
        return Props.Create(() => new SendMessageActor(logger, emailProvider, smsProvider));
    }

    /// <summary>
    /// Handles the SendMessageCommand for both email and SMS delivery
    /// </summary>
    /// <param name="command">The message command to process</param>
    private void HandleSendMessage(SendMessageCommand command)
    {
        try
        {
            _logger.LogInformation("📨 Received message command of type {MessageType}", command.MessageType);

            switch (command.MessageType)
            {
                case MessageType.Email:
                    HandleEmailSend(command);
                    break;
                        
                case MessageType.SMS:
                    HandleSmsSend(command);
                    break;
                        
                case MessageType.Both:
                    HandleDualChannelSend(command);
                    break;
                    
                default:
                    _logger.LogWarning("⚠️ Unknown message type: {MessageType}", command.MessageType);
                    break;
            }
                
            _logger.LogInformation("✅ Finished processing message command of type {MessageType}", command.MessageType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Exception while processing message command");
        }
    }

    private void HandleEmailSend(SendMessageCommand command)
    {
        try
        {
            _logger.LogInformation("📧 Starting email send to {ToEmails} | Subject: {Subject}", 
                JsonSerializer.Serialize(command.ToEmails), command.Subject);

            if (string.IsNullOrEmpty(command.Template))
            {
                _emailProvider.SendEmailAsync(command.ToEmails, command.Subject, command.Body)
                    .GetAwaiter().GetResult();
                _logger.LogInformation("✅ Email sent successfully to {ToEmails}", JsonSerializer.Serialize(command.ToEmails));
            }
            else
            {
                var formattedBody = ReplacePlaceholders(command.Template, command.Replacements);
                _emailProvider.SendEmailAsync(command.ToEmails, command.Subject, formattedBody)
                    .GetAwaiter().GetResult();
                _logger.LogInformation("✅ Templated email sent successfully to {ToEmails}", JsonSerializer.Serialize(command.ToEmails));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send email to {ToEmails}", JsonSerializer.Serialize(command.ToEmails));
        }
    }

    private void HandleSmsSend(SendMessageCommand command)
    {
        try
        {
            _logger.LogInformation("📱 Starting SMS send to {ToPhoneNumbers}", 
                JsonSerializer.Serialize(command.ToPhoneNumbers));

            if (string.IsNullOrEmpty(command.Template))
            {
                var (success, response) = _smsProvider.SendSmsAsync(command.ToPhoneNumbers, command.Body)
                    .GetAwaiter().GetResult();

                if (success)
                    _logger.LogInformation("✅ SMS sent successfully to {ToPhoneNumbers}. API Response: {Response}", JsonSerializer.Serialize(command.ToPhoneNumbers), response);
                else
                    _logger.LogWarning("⚠️ SMS sending failed to {ToPhoneNumbers}. API Response: {Response}", JsonSerializer.Serialize(command.ToPhoneNumbers), response);
            }
            else
            {
                var formattedMessage = ReplacePlaceholders(command.Template, command.Replacements);
                var (success, response) = _smsProvider.SendSmsAsync(command.ToPhoneNumbers, formattedMessage)
                    .GetAwaiter().GetResult();

                if (success)
                    _logger.LogInformation("✅ Templated SMS sent successfully to {ToPhoneNumbers}. API Response: {Response}", JsonSerializer.Serialize(command.ToPhoneNumbers), response);
                else
                    _logger.LogWarning("⚠️ Templated SMS sending failed to {ToPhoneNumbers}. API Response: {Response}", JsonSerializer.Serialize(command.ToPhoneNumbers), response);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send SMS to {ToPhoneNumbers}", JsonSerializer.Serialize(command.ToPhoneNumbers));
        }
    }

    private void HandleDualChannelSend(SendMessageCommand command)
    {
        _logger.LogInformation("📬 Starting dual-channel message to {ToEmails} and {ToPhoneNumbers}", 
            JsonSerializer.Serialize(command.ToEmails), JsonSerializer.Serialize(command.ToPhoneNumbers));
                            
        try
        {
            if (string.IsNullOrEmpty(command.Template))
            {
                _emailProvider.SendEmailAsync(command.ToEmails, command.Subject, command.Body)
                    .GetAwaiter().GetResult();
                _logger.LogInformation("✅ Email part sent successfully");

                var (success, response) = _smsProvider.SendSmsAsync(command.ToPhoneNumbers, command.Body)
                    .GetAwaiter().GetResult();

                if (success)
                    _logger.LogInformation("✅ SMS part sent successfully. API Response: {Response}", response);
                else
                    _logger.LogWarning("⚠️ SMS part failed. API Response: {Response}", response);
            }
            else
            {
                var formattedContent = ReplacePlaceholders(command.Template, command.Replacements);

                _emailProvider.SendEmailAsync(command.ToEmails, command.Subject, formattedContent)
                    .GetAwaiter().GetResult();
                _logger.LogInformation("✅ Templated email part sent successfully");

                var (success, response) = _smsProvider.SendSmsAsync(command.ToPhoneNumbers, formattedContent)
                    .GetAwaiter().GetResult();

                if (success)
                    _logger.LogInformation("✅ Templated SMS part sent successfully. API Response: {Response}", response);
                else
                    _logger.LogWarning("⚠️ Templated SMS part failed. API Response: {Response}", response);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error sending dual-channel message");
        }
    }
        
    private string ReplacePlaceholders(string template, Dictionary<string, string> replacements)
    {
        try
        {
            foreach (var replacement in replacements)
            {
                template = Regex.Replace(template, 
                    $"{{{{{replacement.Key}}}}}", 
                    replacement.Value, 
                    RegexOptions.IgnoreCase);
            }
            _logger.LogInformation("🛠️ Placeholder replacements done successfully");
            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error replacing placeholders in template");
            return template; // Return raw if failed
        }
    }
}
