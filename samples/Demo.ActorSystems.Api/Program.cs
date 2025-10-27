using QFace.Sdk.ActorSystems;
using QFace.Sdk.Logging;
using Demo.ActorSystems.Api.Actors;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure QFace logging with Graylog
builder.Host.AddQFaceLogging();

// Configure Actor System with lifecycle management
builder.Services.AddActorSystemWithLifecycle(
    new[] { typeof(Program).Assembly },
    actorConfig => builder.Configuration.GetSection("ActorSystem").Bind(actorConfig)
);

var app = builder.Build();

// Log application startup
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("QFace Actor Systems Demo API starting up...");

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Actor Systems Demo API v1"));
    logger.LogInformation("Swagger UI enabled for development environment");
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

// Enable Actor System
app.UseActorSystem();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => new { status = "healthy", timestamp = DateTime.UtcNow })
    .WithName("HealthCheck");

// Actor system test endpoints
app.MapGet("/actor-system/test", (IActorService actorService) =>
{
    try
    {
        // Send a test message to the test actor
        actorService.Tell<TestActor>("Hello from HTTP endpoint!");
        
        return Results.Ok(new { 
            message = "Test message sent to actor system",
            timestamp = DateTime.UtcNow 
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Actor system test failed");
        return Results.Problem("Actor system test failed");
    }
})
.WithName("ActorSystemTest");

logger.LogInformation("QFace Actor Systems Demo API configured successfully");

app.Run();

