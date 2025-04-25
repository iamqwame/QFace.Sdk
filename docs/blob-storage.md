# QFace.Sdk.SendMessage

This package provides a convenient way to send messages (currently focused on email) in .NET applications using an actor-based approach for improved scalability and resilience.

## Features

- Send emails to single or multiple recipients
- Support for plain text or HTML emails
- Template-based emails with placeholder replacements
- Actor-based processing for non-blocking operations
- Graceful fallback to direct sending if actor system is unavailable

## Installation

```shell
dotnet add package QFace.Sdk.SendMessage
```

## Configuration

Add the following configuration to your `appsettings.json`:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.example.com",
    "SmtpPort": 587,
    "SmtpUser": "your-username",
    "SmtpPassword": "your-password",
    "FromEmail": "noreply@example.com",
    "FromName": "Your Application"
  }
}
```

## Usage

### Registration

```csharp
// In Program.cs or Startup.cs
builder.Services.AddEmailServices();

// After building the app
app.Services.UseEmailServices();
```

### Basic Usage

```csharp
// Inject IServiceProvider into your controllers or services
public class NotificationController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;

    public NotificationController(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [HttpPost("notify")]
    public IActionResult SendNotification(string email, string subject, string message)
    {
        // Create a simple email command
        var command = SendEmailCommand.Create(email, subject, message);

        // Send the email (handled by the actor system)
        _serviceProvider.SendEmail(command);

        return Ok();
    }

    [HttpPost("welcome")]
    public IActionResult SendWelcomeEmail(string email, string name)
    {
        // Create a templated email command
        var template = "<h1>Welcome, {{Name}}!</h1><p>Thank you for joining our service.</p>";
        var replacements = new Dictionary<string, string> { { "Name", name } };

        var command = SendEmailCommand.CreateWithTemplate(
            email,
            "Welcome to Our Service",
            template,
            replacements
        );

        // Send the email (handled by the actor system)
        _serviceProvider.SendEmail(command);

        return Ok();
    }
}
```

## Advanced Use Cases

### Multiple Recipients

```csharp
// Send to multiple recipients
var recipients = new List<string> { "user1@example.com", "user2@example.com" };
var command = SendEmailCommand.Create(
    recipients,
    "Important Announcement",
    "<p>This is an important announcement.</p>"
);
_serviceProvider.SendEmail(command);
```

### Direct Use of EmailService

```csharp
// Inject IEmailService directly if you need more control
public class EmailController : ControllerBase
{
    private readonly IEmailService _emailService;

    public EmailController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost("critical-notification")]
    public async Task<IActionResult> SendCriticalNotification(string email, string message)
    {
        // Directly use the email service for synchronous processing
        var result = await _emailService.SendEmailAsync(
            new List<string> { email },
            "CRITICAL NOTIFICATION",
            $"<p>{message}</p>"
        );

        return result ? Ok() : StatusCode(500, "Failed to send email");
    }
}
```

### Custom Actor System Configuration

```csharp
// Configure your actor system with custom settings
builder.Services.AddEmailServices(config =>
{
    config.SystemName = "MyCustomEmailSystem";
    config.ConfigureLogging = true;
});
```
