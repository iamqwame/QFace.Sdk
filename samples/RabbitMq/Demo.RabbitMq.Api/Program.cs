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