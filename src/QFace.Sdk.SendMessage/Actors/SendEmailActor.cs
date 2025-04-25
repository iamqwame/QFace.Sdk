namespace QFace.Sdk.SendMessage.Actors;

/// <summary>
/// Actor for handling email sending operations
/// </summary>
public class SendEmailActor : BaseActor
{
    private readonly ILogger<SendEmailActor> _logger;
    private readonly IEmailService _emailService;

    public SendEmailActor(ILogger<SendEmailActor> logger, IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
        ReceiveAsync<SendEmailCommand>(HandleSendEmail);
    }

    /// <summary>
    /// Handles the SendEmailCommand message
    /// </summary>
    /// <param name="command">The email command to process</param>
    private async Task HandleSendEmail(SendEmailCommand command)
    {
        try
        {
            _logger.LogInformation("📩 Processing email request to {ToEmail}", JsonSerializer.Serialize(command.ToEmails));
        
            if (string.IsNullOrEmpty(command.Template) || command.Replacements.Count == 0)
            {
                _logger.LogInformation("📧 Sending plain email to {ToEmail} with subject: {Subject}", 
                    JsonSerializer.Serialize(command.ToEmails), command.Subject);
                await _emailService.SendEmailAsync(command.ToEmails, command.Subject, command.Body);
            }
            else
            {
                _logger.LogInformation("📨 Sending templated email to {ToEmail} with subject: {Subject}", 
                    JsonSerializer.Serialize(command.ToEmails), command.Subject);
                await _emailService.SendEmailWithTemplateAsync(command.ToEmails, command.Subject, command.Template, command.Replacements);
            }
        
            _logger.LogInformation("✅ Email successfully sent to {ToEmail}", JsonSerializer.Serialize(command.ToEmails));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send email to {ToEmail}", JsonSerializer.Serialize(command.ToEmails));
        }
    }
}