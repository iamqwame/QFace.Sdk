namespace QFace.Sdk.SendMessage.Services;

/// <summary>
/// Implementation of email service using MailKit
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> SendEmailAsync(List<string> toEmail, string subject, string body)
    {
        return await SendEmailInternalAsync(toEmail, subject, body);
    }

    /// <inheritdoc />
    public async Task<bool> SendEmailWithTemplateAsync(List<string> toEmail, string subject, string template, Dictionary<string, string> replacements)
    {
        try
        {
            var formattedBody = ReplacePlaceholders(template, replacements);
            return await SendEmailInternalAsync(toEmail, subject, formattedBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error processing email template for {ToEmail}", toEmail);
            return false;
        }
    }

    private async Task<bool> SendEmailInternalAsync(List<string> toEmails, string subject, string body)
    {
        try
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]!);
            var smtpUser = _configuration["EmailSettings:SmtpUser"];
            var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
            var fromEmail = _configuration["EmailSettings:FromEmail"];
            var fromName = _configuration["EmailSettings:FromName"];

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(fromName, fromEmail));
            foreach (var to in toEmails)
            {
                email.To.Add(new MailboxAddress(to, to));
            }
            email.Subject = subject;
            email.Body = new TextPart("html") { Text = body };

            using var smtp = new SmtpClient();

            _logger.LogInformation("📤 Connecting to SMTP server {SmtpServer}:{SmtpPort}...", smtpServer, smtpPort);
            await smtp.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(smtpUser, smtpPassword);

            _logger.LogInformation("📩 Sending email to {ToEmail} | Subject: {Subject}", toEmails, subject);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("✅ Email sent successfully to {ToEmail}", toEmails);
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
            _logger.LogError(ex, "❌ Unexpected error sending email to {ToEmail}", toEmails);
            return false;
        }
    }

    private string ReplacePlaceholders(string template, Dictionary<string, string> replacements)
    {
        foreach (var replacement in replacements)
        {
            template = Regex.Replace(template, $"{{{{{replacement.Key}}}}}", replacement.Value, RegexOptions.IgnoreCase);
        }
        return template;
    }
}