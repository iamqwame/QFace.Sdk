using Microsoft.AspNetCore.Mvc;
using QFace.Sdk.ActorSystems;
using QFace.Sdk.Kafka.Extensions;
using QFace.Sdk.Kafka.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var assembly = typeof(Program).Assembly;

// Add Actor System
builder.Services.AddActorSystemWithLifecycle(
    [assembly],
    actorConfig => builder.Configuration.GetSection(nameof(ActorConfig)).Bind(actorConfig)
);

// Add Kafka with producer capability
builder.Services.AddKafka(
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

// Initialize Kafka in API mode
app.UseKafkaInApi();

app.MapControllers();

// Add minimal API endpoints for testing Kafka producer
app.MapPost("/publish", async (
    [FromBody] EventSourceModel eventData,
    [FromServices] IKafkaProducer producer) =>
{
    try
    {
        var result = await producer.ProduceAsync("demo.events", eventData, key: eventData.EventType);
        return Results.Ok(new { 
            success = true, 
            message = "Event published successfully",
            topic = result.Topic,
            partition = result.Partition.Value,
            offset = result.Offset.Value,
            eventId = eventData.Id
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapPost("/publish-batch", async (
    [FromBody] List<EventSourceModel> events,
    [FromServices] IKafkaProducer producer) =>
{
    try
    {
        var results = await producer.ProduceBatchAsync("demo.events", events, 
            evt => evt.EventType);
        
        return Results.Ok(new { 
            success = true, 
            message = $"Published {events.Count} events successfully",
            results = results.Select(r => new {
                topic = r.Topic,
                partition = r.Partition.Value,
                offset = r.Offset.Value
            }).ToList()
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapPost("/publish-to-topic", async (
    [FromBody] PublishRequest request,
    [FromServices] IKafkaProducer producer) =>
{
    try
    {
        var result = await producer.ProduceAsync(request.Topic, request.Data, key: request.Key);
        return Results.Ok(new { 
            success = true, 
            message = "Data published successfully",
            topic = result.Topic,
            partition = result.Partition.Value,
            offset = result.Offset.Value
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.Run();

// Event Models
public class EventSourceModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EventType { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Source { get; set; } = "Demo.Kafka.Api";
    public object Data { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class PublishRequest
{
    public string Topic { get; set; }
    public string Key { get; set; }
    public object Data { get; set; }
}

// Sample event types for testing
public class UserCreatedEvent
{
    public string UserId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class OrderCreatedEvent
{
    public string OrderId { get; set; }
    public string CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class OrderItem
{
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}

public class SystemMetricEvent
{
    public string MetricName { get; set; }
    public double Value { get; set; }
    public string Unit { get; set; }
    public Dictionary<string, string> Tags { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
