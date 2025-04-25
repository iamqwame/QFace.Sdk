using QFace.Sdk.ActorSystems;
using QFace.Sdk.SendMessage.Actors;
using QFace.Sdk.SendMessage.Extensions;
using QFace.Sdk.SendMessage.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Email Services
builder.Services.AddEmailServices(config => {
    config.SystemName = "EmailDemo";
});

// Add logging
builder.Services.AddLogging(logging => {
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Email Demo API v1"));
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();


// API Endpoints
app.MapControllers();

// Simple Email Endpoints
app.MapPost("/api/email/simple", async (HttpContext context, IActorService actorService) => {
    var request = await context.Request.ReadFromJsonAsync<SimpleEmailRequest>();
    
    if (request == null || string.IsNullOrEmpty(request.ToEmail) || string.IsNullOrEmpty(request.Subject))
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new { error = "Invalid request parameters" });
        return;
    }
    
    var command = SendEmailCommand.Create(
        request.ToEmail,
        request.Subject,
        request.Body ?? "Empty email body"
    );
    actorService.Tell<SendEmailActor>(command);
    
    
    await context.Response.WriteAsJsonAsync(new { message = "Email sending in progress" });
});

// Templated Email Endpoint
app.MapPost("/api/email/template", async (HttpContext context,  IActorService actorService) => {
    var request = await context.Request.ReadFromJsonAsync<TemplatedEmailRequest>();
    
    if (request == null || string.IsNullOrEmpty(request.ToEmail) || 
        string.IsNullOrEmpty(request.Subject) || string.IsNullOrEmpty(request.Template))
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new { error = "Invalid request parameters" });
        return;
    }
    
    var command = SendEmailCommand.CreateWithTemplate(
        request.ToEmail,
        request.Subject,
        request.Template,
        request.Replacements
    );
    actorService.Tell<SendEmailActor>(command);
    
    await context.Response.WriteAsJsonAsync(new { message = "Templated email sending in progress" });
});

// Bulk Email Endpoint
app.MapPost("/api/email/bulk", async (HttpContext context,IActorService actorService) => {
    var request = await context.Request.ReadFromJsonAsync<BulkEmailRequest>();
    
    if (request?.ToEmails == null || request.ToEmails.Count == 0 || 
        string.IsNullOrEmpty(request.Subject))
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new { error = "Invalid request parameters" });
        return;
    }
    
    var command = SendEmailCommand.Create(
        request.ToEmails,
        request.Subject,
        request.Body ?? "Empty email body"
    );
    
    actorService.Tell<SendEmailActor>(command);
    
    await context.Response.WriteAsJsonAsync(new { message = "Bulk email sending in progress" });
});

// Run the application
app.Run();

// Request DTOs
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