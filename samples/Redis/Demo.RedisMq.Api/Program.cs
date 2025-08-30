using Microsoft.AspNetCore.Mvc;
using QFace.Sdk.ActorSystems;
using QFace.Sdk.RedisMq.Extension;
using QFace.Sdk.RedisMq.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var assembly = typeof(Program).Assembly;

builder.Services.AddActorSystemWithLifecycle(
    new[] { assembly },
    actorConfig => builder.Configuration.GetSection(nameof(ActorConfig)).Bind(actorConfig)
);

builder.Services.AddRedisMq(
    builder.Configuration,
    new[] { assembly }
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
app.UseRedisMqInApi();
app.UseActorSystem();

app.MapControllers();

// Add minimal API endpoints for testing
app.MapPost("/publish", async (
    [FromBody] MessageDto message,
    [FromServices] IRedisMqPublisher publisher) =>
{
    try
    {
        bool success = await publisher.PublishAsync(message, "demo_channel");
        return Results.Ok(new { success, message = "Message published successfully" });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapPost("/publish-important", async (
    [FromBody] MessageDto message,
    [FromServices] IRedisMqPublisher publisher) =>
{
    try
    {
        bool success = await publisher.PublishAsync(message, "important_messages");
        return Results.Ok(new { success, message = "Important message published successfully" });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapPost("/publish-normal", async (
    [FromBody] MessageDto message,
    [FromServices] IRedisMqPublisher publisher) =>
{
    try
    {
        bool success = await publisher.PublishAsync(message, "normal_messages");
        return Results.Ok(new { success, message = "Normal message published successfully" });
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
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}