using Microsoft.AspNetCore.Mvc;
using QFace.Sdk.ActorSystems;
using QFace.Sdk.RabbitMq.Extension;
using QFace.Sdk.RabbitMq.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var assembly = typeof(Program).Assembly;


builder.Services.AddActorSystemWithLifecycle(
    [assembly],
    actorConfig => builder.Configuration.GetSection(nameof(ActorConfig)).Bind(actorConfig)
);

builder.Services.AddRabbitMq(
    builder.Configuration,
    [assembly]
);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Initialize both systems independently
app.UseRabbitMqInApi();
app.UseActorSystem();

app.MapControllers();

// Add minimal API endpoints for testing
app.MapPost("/publish", async (
    [FromBody] MessageDto message,
    [FromServices] IRabbitMqPublisher publisher) =>
{
    try
    {
        
       bool success = await publisher.PublishAsync(message, "demo_exchange");
        return Results.Ok(new { success, message = "Message published successfully" });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.Run();


public class MessageDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Content { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class UnifiedMessageModel
{
    // Required for all message types
    public string MessageType { get; set; }
    
    // Email properties
    public string ToEmail { get; set; }
    public List<string> ToEmails { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    
    // Template properties
    public string Template { get; set; }
    public Dictionary<string, string> Replacements { get; set; }
    
    // SMS properties
    public string PhoneNumber { get; set; }
    public List<string> PhoneNumbers { get; set; }
    public string Message { get; set; }
    
    // Combined properties
    public string Email { get; set; }
    
    // Optional metadata
    public string MessageId { get; set; }
    public string CorrelationId { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
    
    // Timestamp for tracking
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}
