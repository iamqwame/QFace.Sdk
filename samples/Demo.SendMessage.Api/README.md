# QFace.Sdk.SendMessage Demo API

This is a minimal API example to demonstrate how to use the QFace.Sdk.SendMessage library to send emails via an actor-based system.

## Features

- Send simple emails to a single recipient
- Send templated emails with replaceable placeholders
- Send bulk emails to multiple recipients
- Uses actor system for asynchronous email processing

## Setup

1. Clone this repository
2. Update the `appsettings.json` file with your SMTP server details
3. Build and run the project
4. Navigate to the Swagger UI at `/swagger` to test the API endpoints

## Configuration

Edit the `appsettings.json` file to configure your email settings:

```json
"EmailSettings": {
  "SmtpServer": "smtp.example.com",
  "SmtpPort": 587,
  "SmtpUser": "your-user@example.com",
  "SmtpPassword": "your-password",
  "FromEmail": "noreply@yourdomain.com",
  "FromName": "Demo Email Service"
}
```

## API Endpoints

### Send Simple Email

```
POST /api/email/simple
```

Request body:
```json
{
  "toEmail": "recipient@example.com",
  "subject": "Hello World",
  "body": "<h1>This is a test email</h1><p>Hello from the QFace.Sdk.SendMessage demo!</p>"
}
```

### Send Templated Email

```
POST /api/email/template
```

Request body:
```json
{
  "toEmail": "recipient@example.com",
  "subject": "Welcome to Our Service",
  "template": "<html><body><h1>Hello, {{Name}}!</h1><p>Welcome to {{CompanyName}}.</p></body></html>",
  "replacements": {
    "Name": "John Doe",
    "CompanyName": "QFace Demo"
  }
}
```

### Send Bulk Email

```
POST /api/email/bulk
```

Request body:
```json
{
  "toEmails": ["recipient1@example.com", "recipient2@example.com"],
  "subject": "Important Announcement",
  "body": "<h1>Important Announcement</h1><p>This is a test bulk email.</p>"
}
```

## Template Example

The project includes a sample email template (`welcome-template.html`) that demonstrates how to use HTML templates with placeholders.

To use this template, read the content of the file and pass it as the `template` parameter, along with appropriate replacements.

## How It Works

1. The application uses the `AddEmailServices()` extension method to register the email service and actor system
2. The `UseEmailServices()` method initializes the email actor
3. API endpoints accept email requests and use the `SendEmail()` extension method to tell the actor to process the email
4. The actor handles sending the email asynchronously
5. If the actor system fails, a fallback mechanism directly calls the email service

## Error Handling

- The email service logs comprehensive error information
- SMTP-specific errors are captured and logged separately
- The API returns appropriate HTTP status codes for invalid requests

## Dependencies

- QFace.Sdk.SendMessage
- QFace.Sdk.ActorSystems
- MailKit for the actual email sending