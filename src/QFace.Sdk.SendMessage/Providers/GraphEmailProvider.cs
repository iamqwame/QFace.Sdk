namespace QFace.Sdk.SendMessage.Providers;

/// <summary>
/// Email provider implementation using Microsoft Graph API
/// </summary>
public class GraphEmailProvider : IEmailProvider
{
    private readonly ILogger<GraphEmailProvider> _logger;
    private readonly EmailConfig _config;
    private GraphServiceClient? _graphClient;
    private bool _isConfigured;

    public string ProviderName => "Microsoft Graph";

    /// <summary>
    /// Initializes a new instance of the GraphEmailProvider
    /// </summary>
    /// <param name="options">Configuration options containing email settings (TenantId, ClientId, ClientSecret, etc.)</param>
    /// <param name="logger">Logger instance for logging operations</param>
    public GraphEmailProvider(IOptions<MessageConfig> options, ILogger<GraphEmailProvider> logger)
    {
        _logger = logger;
        _config = options.Value.Email;
        Initialize().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Initializes the Microsoft Graph email provider by validating configuration and creating the Graph client
    /// </summary>
    /// <returns>True if initialization was successful, otherwise false</returns>
    /// <remarks>
    /// This method validates that the Provider is set to "Graph", checks for required configuration
    /// (TenantId, ClientId, ClientSecret, SendAsUser/FromEmail), and creates a GraphServiceClient
    /// using client credentials authentication.
    /// </remarks>
    public async Task<bool> Initialize()
    {
        try
        {
            // Check if Graph provider is selected
            if (_config.Provider?.ToUpperInvariant() != "GRAPH")
            {
                _logger.LogInformation("📧 Graph provider not selected (Provider: {Provider})", _config.Provider);
                _isConfigured = false;
                return false;
            }

            // Validate required Graph settings
            if (string.IsNullOrEmpty(_config.TenantId) ||
                string.IsNullOrEmpty(_config.ClientId) ||
                string.IsNullOrEmpty(_config.ClientSecret))
            {
                _logger.LogWarning("⚠️ Graph provider not fully configured. Missing TenantId, ClientId, or ClientSecret");
                _isConfigured = false;
                return false;
            }

            // Validate FromEmail or SendAsUser
            var sendAsUser = !string.IsNullOrEmpty(_config.SendAsUser) 
                ? _config.SendAsUser 
                : _config.FromEmail;

            if (string.IsNullOrEmpty(sendAsUser))
            {
                _logger.LogWarning("⚠️ Graph provider requires FromEmail or SendAsUser");
                _isConfigured = false;
                return false;
            }

            // Create Graph client with client credentials
            var credential = new ClientSecretCredential(
                _config.TenantId,
                _config.ClientId,
                _config.ClientSecret
            );

            _graphClient = new GraphServiceClient(credential);
            _isConfigured = true;

            _logger.LogInformation("📧 Microsoft Graph provider configured. Sending as: {SendAsUser}", sendAsUser);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error initializing Microsoft Graph provider: {Message}", ex.Message);
            _isConfigured = false;
            return false;
        }
    }

    /// <summary>
    /// Sends an email to one or more recipients using Microsoft Graph API
    /// </summary>
    /// <param name="toEmail">List of recipient email addresses</param>
    /// <param name="subject">Email subject line</param>
    /// <param name="body">Email body content in HTML format</param>
    /// <returns>True if the email was sent successfully, otherwise false</returns>
    /// <remarks>
    /// This method sends emails using Microsoft Graph API with application permissions.
    /// The email is sent on behalf of the user specified in SendAsUser or FromEmail configuration.
    /// The From field is set to the configured FromEmail and FromName values.
    /// </remarks>
    public async Task<bool> SendEmailAsync(List<string> toEmail, string subject, string body)
    {
        if (!_isConfigured || _graphClient == null)
        {
            _logger.LogError("❌ Cannot send email: Graph provider not properly configured");
            return false;
        }

        try
        {
            var sendAsUser = !string.IsNullOrEmpty(_config.SendAsUser) 
                ? _config.SendAsUser 
                : _config.FromEmail;

            _logger.LogInformation("📤 Sending email via Microsoft Graph to {ToEmail} | Subject: {Subject}", 
                string.Join(", ", toEmail), subject);

            var message = new Message
            {
                Subject = subject,
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = body
                },
                // Set From field to the desired email and display name
                // Note: SendAsUser must match the mailbox's PRIMARY address (nobert@qfacesolutions.com)
                // FromEmail can be an alias (noreply@qfacesolutions.com) - Graph API will use it if valid
                // However, Microsoft Graph may override the From field to match the mailbox owner
                From = new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = _config.FromEmail, // noreply@qfacesolutions.com (alias)
                        Name = _config.FromName      // "QIM ERP Notifications"
                    }
                },
                ToRecipients = toEmail.Select(email => new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = email
                    }
                }).ToList()
            };

            // Send email as the configured user
            await _graphClient.Users[sendAsUser]
                .SendMail
                .PostAsync(new Microsoft.Graph.Users.Item.SendMail.SendMailPostRequestBody
                {
                    Message = message,
                    SaveToSentItems = true
                });

            _logger.LogInformation("✅ Email sent successfully via Microsoft Graph to {ToEmail}", string.Join(", ", toEmail));
            return true;
        }
        catch (Microsoft.Graph.Models.ODataErrors.ODataError odataError)
        {
            var sendAsUser = !string.IsNullOrEmpty(_config.SendAsUser) 
                ? _config.SendAsUser 
                : _config.FromEmail;
                
            _logger.LogError("❌ Graph API Error sending email to {ToEmail}", toEmail);
            _logger.LogError("   Error Code: {ErrorCode}", odataError.Error?.Code);
            _logger.LogError("   Error Message: {ErrorMessage}", odataError.Error?.Message);
            if (odataError.Error?.Details != null)
            {
                foreach (var detail in odataError.Error.Details)
                {
                    _logger.LogError("   Detail: {Detail}", detail.Message);
                }
            }
            
            // Provide helpful error messages
            if (odataError.Error?.Code == "Authorization_RequestDenied" || 
                odataError.Error?.Message?.Contains("Access is denied") == true)
            {
                _logger.LogError("   ⚠ This usually means:");
                _logger.LogError("      1. Mail.Send permission not granted with admin consent");
                _logger.LogError("      2. Permission hasn't propagated yet (wait 1-2 minutes)");
                _logger.LogError("      3. User {SendAsUser} doesn't exist or app can't access it", sendAsUser);
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Unexpected error sending email via Microsoft Graph to {ToEmail}", toEmail);
            return false;
        }
    }
}
