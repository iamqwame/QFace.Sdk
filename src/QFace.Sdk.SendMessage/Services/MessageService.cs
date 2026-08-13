namespace QFace.Sdk.SendMessage.Services;

/// <summary>
/// Implementation of the unified message service using actors
/// </summary>
public class MessageService : IMessageService
{
    private readonly ILogger<MessageService> _logger;
    private readonly IActorService _actorService;

    public MessageService(
        ILogger<MessageService> logger,
        IActorService actorService)
    {
        _logger = logger;
        _actorService = actorService;
    }

    #region Email Methods
    /// <inheritdoc />
    public Task<bool> SendEmailAsync(List<string> toEmail, string subject, string body)
    {
        try
        {
            _logger.LogInformation("Creating email request to {ToEmail} | Subject: {Subject}", toEmail, subject);
                
            var command = SendMessageCommand.CreateEmailOnly(toEmail, subject, body);
            _actorService.Tell<SendMessageActor>(command);
                
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating email request to {ToEmail}", toEmail);
            return Task.FromResult(false);
        }
    }

    /// <inheritdoc />
    public Task<bool> SendEmailWithTemplateAsync(List<string> toEmail, string subject, 
        string template, Dictionary<string, string> replacements)
    {
        try
        {
            _logger.LogInformation("Creating templated email request to {ToEmail} | Subject: {Subject}", toEmail, subject);
                
            var command = SendMessageCommand.CreateEmailWithTemplateOnly(toEmail, subject, template, replacements);
            _actorService.Tell<SendMessageActor>(command);
                
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating templated email request for {ToEmail}", toEmail);
            return Task.FromResult(false);
        }
    }
    #endregion

    #region SMS Methods
    /// <inheritdoc />
    public Task<bool> SendSmsAsync(List<string> toPhoneNumbers, string message)
    {
        try
        {
            _logger.LogInformation("Creating SMS request to {ToPhoneNumbers}", toPhoneNumbers);
                
            var command = SendMessageCommand.CreateSMSOnly(toPhoneNumbers, message);
            _actorService.Tell<SendMessageActor>(command);
                
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating SMS request to {ToPhoneNumbers}", toPhoneNumbers);
            return Task.FromResult(false);
        }
    }

    /// <inheritdoc />
    public Task<bool> SendSmsWithTemplateAsync(List<string> toPhoneNumbers, string template, 
        Dictionary<string, string> replacements)
    {
        try
        {
            _logger.LogInformation("Creating templated SMS request to {ToPhoneNumbers}", toPhoneNumbers);
                
            var command = SendMessageCommand.CreateSMSWithTemplateOnly(toPhoneNumbers, template, replacements);
            _actorService.Tell<SendMessageActor>(command);
                
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating templated SMS request for {ToPhoneNumbers}", toPhoneNumbers);
            return Task.FromResult(false);
        }
    }
    #endregion

    #region Combined Methods
    /// <inheritdoc />
    public Task<bool> SendBothAsync(List<string> toEmail, List<string> toPhoneNumbers, 
        string subject, string body)
    {
        try
        {
            _logger.LogInformation("Creating dual-channel message to {ToEmail} and {ToPhoneNumbers}", 
                toEmail, toPhoneNumbers);
                
            var command = SendMessageCommand.CreateBoth(toEmail[0], toPhoneNumbers[0], subject, body);
            _actorService.Tell<SendMessageActor>(command);
                
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating dual-channel message");
            return Task.FromResult(false);
        }
    }

    /// <inheritdoc />
    public Task<bool> ProcessMessageCommandAsync(SendMessageCommand command)
    {
        try
        {
            _logger.LogInformation("Processing message command via actor");
            _actorService.Tell<SendMessageActor>(command);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message command");
            return Task.FromResult(false);
        }
    }
    #endregion
}