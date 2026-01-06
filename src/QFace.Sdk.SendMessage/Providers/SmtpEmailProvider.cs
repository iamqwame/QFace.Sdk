namespace QFace.Sdk.SendMessage.Providers;

/// <summary>
/// Email provider implementation using SMTP via MailKit
/// </summary>
public class SmtpEmailProvider : IEmailProvider
{
    private readonly ILogger<SmtpEmailProvider> _logger;
    private readonly EmailConfig _config;
    private bool _isConfigured;

    public string ProviderName => "SMTP";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public SmtpEmailProvider(IOptions<MessageConfig> options, ILogger<SmtpEmailProvider> logger)
    {
        _logger = logger;
        _config = options.Value.Email;
        Initialize().GetAwaiter().GetResult();
    }

    public async Task<bool> Initialize()
    {
        try
        {
            // For localhost (MailPit), only require SmtpServer and FromEmail
            if (_config.IsLocalHost)
            {
                _isConfigured = !string.IsNullOrEmpty(_config.SmtpServer) &&
                                !string.IsNullOrEmpty(_config.FromEmail);
            }
            else
            {
                _isConfigured = !string.IsNullOrEmpty(_config.SmtpServer) &&
                                !string.IsNullOrEmpty(_config.SmtpUser) &&
                                !string.IsNullOrEmpty(_config.SmtpPassword) &&
                                !string.IsNullOrEmpty(_config.FromEmail);
            }

            if (_isConfigured)
            {
                var mode = _config.IsLocalHost ? "localhost" : "production";
                _logger.LogInformation("📧 SMTP provider configured for {Mode}: {Server}:{Port}", mode, _config.SmtpServer, _config.SmtpPort);
            }
            else
            {
                _logger.LogWarning("⚠️ SMTP provider not fully configured");
            }

            return _isConfigured;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error initializing SMTP provider: {Message}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="toEmail"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    public async Task<bool> SendEmailAsync(List<string> toEmail, string subject, string body)
    {
        if (!_isConfigured)
        {
            _logger.LogError("❌ Cannot send email: SMTP provider not properly configured");
            return false;
        }
            
        try
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_config.FromName, _config.FromEmail));
                
            foreach (var to in toEmail)
            {
                email.To.Add(new MailboxAddress(to, to));
            }
                
            email.Subject = subject;
            email.Body = new TextPart("html") { Text = body };

            using var smtp = new SmtpClient();

            _logger.LogInformation("📤 Connecting to SMTP server {SmtpServer}:{SmtpPort}...", _config.SmtpServer, _config.SmtpPort);

            if (_config.IsLocalHost)
            {
                // For localhost (MailPit), connect without TLS
                await smtp.ConnectAsync(_config.SmtpServer, _config.SmtpPort, SecureSocketOptions.None);
                // Skip authentication for localhost
            }
            else
            {
                // For production SMTP servers
                await smtp.ConnectAsync(_config.SmtpServer, _config.SmtpPort, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_config.SmtpUser, _config.SmtpPassword);
            }

            _logger.LogInformation("📩 Sending email to {ToEmail} | Subject: {Subject}", toEmail, subject);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("✅ Email sent successfully to {ToEmail}", toEmail);
            return true;
        }
        catch (SmtpCommandException ex)
        {
            _logger.LogError("❌ SMTP Error: failed with response {ResponseCode}. Message: {Message}", 
                ex.StatusCode, ex.Message);
            return false;
        }
        catch (SmtpProtocolException ex)
        {
            _logger.LogError("❌ SMTP Protocol Error: {Message}", ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Unexpected error sending email to {ToEmail}", toEmail);
            return false;
        }
    }
}