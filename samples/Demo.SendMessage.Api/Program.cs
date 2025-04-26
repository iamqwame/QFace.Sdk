using QFace.Sdk.ActorSystems;
using QFace.Sdk.SendMessage.Extensions;
using QFace.Sdk.SendMessage.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMessagingServices(builder.Configuration, config => {
    config.SystemName = "MessagingDemo";
});
builder.Services.AddLogging(logging => {
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

var app = builder.Build();

// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Messaging Demo API v1"));
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.UseActorSystem();
app.MapControllers();

// ---------------- EMAIL ----------------

// Send simple email
app.MapPost("/api/email/simple", async (SimpleEmailRequest request, IMessageService messageService) =>
{
    if (request == null || string.IsNullOrEmpty(request.ToEmail) || string.IsNullOrEmpty(request.Subject))
    {
        return Results.BadRequest(new { error = "Invalid request parameters" });
    }

    await messageService.SendEmailAsync(
        new List<string> { request.ToEmail },
        request.Subject,
        request.Body ?? "Empty email body"
    );

    return Results.Ok(new { message = "Email sending in progress" });
});

// Send templated email
app.MapPost("/api/email/template", async (TemplatedEmailRequest request, IMessageService messageService) =>
{
    if (request == null || string.IsNullOrEmpty(request.ToEmail) || 
        string.IsNullOrEmpty(request.Subject) || string.IsNullOrEmpty(request.Template))
    {
        return Results.BadRequest(new { error = "Invalid request parameters" });
    }

    await messageService.SendEmailWithTemplateAsync(
        new List<string> { request.ToEmail },
        request.Subject,
        request.Template,
        request.Replacements ?? new Dictionary<string, string>()
    );

    return Results.Ok(new { message = "Templated email sending in progress" });
});

// Send bulk email
app.MapPost("/api/email/bulk", async (BulkEmailRequest request, IMessageService messageService) =>
{
    if (request?.ToEmails == null || request.ToEmails.Count == 0 || string.IsNullOrEmpty(request.Subject))
    {
        return Results.BadRequest(new { error = "Invalid request parameters" });
    }

    await messageService.SendEmailAsync(
        request.ToEmails,
        request.Subject,
        request.Body ?? "Empty email body"
    );

    return Results.Ok(new { message = "Bulk email sending in progress" });
});

// ---------------- SMS ----------------

// Send single SMS
app.MapPost("/api/sms/send", async (SmsRequest request, IMessageService messageService) =>
{
    if (request == null || string.IsNullOrEmpty(request.PhoneNumber) || string.IsNullOrEmpty(request.Message))
    {
        return Results.BadRequest(new { error = "Invalid request parameters" });
    }

    await messageService.SendSmsAsync(
        new List<string> { request.PhoneNumber },
        request.Message
    );

    return Results.Ok(new { message = "SMS sending in progress" });
});

// Send bulk SMS
app.MapPost("/api/sms/bulk", async (BulkSmsRequest request, IMessageService messageService) =>
{
    if (request?.PhoneNumbers == null || request.PhoneNumbers.Count == 0 || string.IsNullOrEmpty(request.Message))
    {
        return Results.BadRequest(new { error = "Invalid request parameters" });
    }

    await messageService.SendSmsAsync(
        request.PhoneNumbers,
        request.Message
    );

    return Results.Ok(new { message = "Bulk SMS sending in progress" });
});

// ---------------- BOTH ----------------

// Send both email and SMS
app.MapPost("/api/message/both", async (CombinedMessageRequest request, IMessageService messageService) =>
{
    if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.PhoneNumber) ||
        string.IsNullOrEmpty(request.Subject) || string.IsNullOrEmpty(request.Message))
    {
        return Results.BadRequest(new { error = "Invalid request parameters" });
    }

    await messageService.SendBothAsync(
        new List<string> { request.Email },
        new List<string> { request.PhoneNumber },
        request.Subject,
        request.Message
    );

    return Results.Ok(new { message = "Combined message sending in progress" });
});

// Run app
app.Run();

// ---------------- Request DTOs ----------------

public class SimpleEmailRequest
{
    public string ToEmail { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
}

public class TemplatedEmailRequest
{
    public string ToEmail { get; set; }
    public string Subject { get; set; }
    public string Template { get; set; }
    public Dictionary<string, string> Replacements { get; set; }
}

public class BulkEmailRequest
{
    public List<string> ToEmails { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
}

public class SmsRequest
{
    public string PhoneNumber { get; set; }
    public string Message { get; set; }
}

public class BulkSmsRequest
{
    public List<string> PhoneNumbers { get; set; }
    public string Message { get; set; }
}

public class CombinedMessageRequest
{
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Subject { get; set; }
    public string Message { get; set; }
}
