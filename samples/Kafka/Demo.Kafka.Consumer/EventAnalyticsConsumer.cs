using Microsoft.Extensions.Logging;
using QFace.Sdk.Kafka.Models;

namespace Demo.Kafka.Consumer;

/// <summary>
/// Demo Kafka consumer that showcases your exact pattern with topic groups
/// </summary>
public class EventAnalyticsConsumer : KafkaConsumerBase
{
    private readonly ILogger<EventAnalyticsConsumer> _logger;

    public EventAnalyticsConsumer(ILogger<EventAnalyticsConsumer> logger, ITopLevelActors topLevelActors)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles analytics events using topic group configuration
    /// This demonstrates your exact pattern: [ConsumeTopic("TopicGroupName")]
    /// </summary>
    [ConsumeTopic("Analytics")]
    public async Task HandleAnalyticsEvents(List<EventSourceModel> events)
    {
        _logger.LogInformation("=== ANALYTICS BATCH RECEIVED ===");
        _logger.LogInformation($"Batch Size: {events.Count}");
        _logger.LogInformation($"Timestamp: {DateTime.UtcNow}");
        
        foreach (var evt in events)
        {
            _logger.LogInformation($"Processing Event: {evt.Id}");
            _logger.LogInformation($"Event Type: {evt.EventType}");
            _logger.LogInformation($"Source: {evt.Source}");
            _logger.LogInformation($"Data: {System.Text.Json.JsonSerializer.Serialize(evt.Data)}");
            
            // Simulate analytics processing
            await ProcessAnalyticsEvent(evt);
        }
        
        _logger.LogInformation("=== ANALYTICS BATCH PROCESSED ===");
    }

    /// <summary>
    /// Handles user events using topic group
    /// </summary>
    [ConsumeTopic("UserEvents")]
    public async Task HandleUserEvents(List<EventSourceModel> events)
    {
        _logger.LogInformation("--- USER EVENTS BATCH ---");
        _logger.LogInformation($"Received {events.Count} user events");
        
        foreach (var evt in events)
        {
            switch (evt.EventType)
            {
                case "user.created":
                    _logger.LogInformation($"✅ User Created: {evt.Id}");
                    await ProcessUserCreated(evt);
                    break;
                case "user.updated":
                    _logger.LogInformation($"🔄 User Updated: {evt.Id}");
                    await ProcessUserUpdated(evt);
                    break;
                case "user.deleted":
                    _logger.LogInformation($"❌ User Deleted: {evt.Id}");
                    await ProcessUserDeleted(evt);
                    break;
                default:
                    _logger.LogInformation($"🔍 Unknown User Event: {evt.EventType}");
                    break;
            }
        }
        
        _logger.LogInformation("--- USER EVENTS COMPLETED ---");
    }

    /// <summary>
    /// Handles system monitoring events
    /// </summary>
    [ConsumeTopic("SystemMonitoring")]
    public async Task HandleSystemEvents(List<EventSourceModel> events)
    {
        _logger.LogInformation("🖥️ SYSTEM MONITORING BATCH 🖥️");
        _logger.LogInformation($"Processing {events.Count} system events");
        
        foreach (var evt in events)
        {
            if (evt.EventType == "system.metric")
            {
                _logger.LogInformation($"📊 Metric Event: {evt.Id}");
                await ProcessSystemMetric(evt);
            }
            else if (evt.EventType.StartsWith("system.alert"))
            {
                _logger.LogWarning($"🚨 Alert Event: {evt.Id} - {evt.EventType}");
                await ProcessSystemAlert(evt);
            }
        }
        
        _logger.LogInformation("🖥️ SYSTEM MONITORING COMPLETED 🖥️");
    }

    /// <summary>
    /// Handles critical alerts with direct topic specification (bypasses topic groups)
    /// </summary>
    [ConsumeTopic("alerts.critical", "alerts.emergency")]
    public async Task HandleCriticalAlerts(List<EventSourceModel> events)
    {
        _logger.LogCritical("🚨🚨🚨 CRITICAL ALERTS RECEIVED 🚨🚨🚨");
        
        foreach (var evt in events)
        {
            _logger.LogCritical($"CRITICAL: {evt.EventType} - {evt.Id}");
            _logger.LogCritical($"Data: {System.Text.Json.JsonSerializer.Serialize(evt.Data)}");
            
            // Process critical alerts immediately
            await ProcessCriticalAlert(evt);
        }
        
        _logger.LogCritical("🚨🚨🚨 CRITICAL ALERTS PROCESSED 🚨🚨🚨");
    }

    private async Task ProcessAnalyticsEvent(EventSourceModel evt)
    {
        // Simulate analytics processing (store in data warehouse, update dashboards, etc.)
        await Task.Delay(50);
        _logger.LogDebug($"Analytics processed for event {evt.Id}");
    }

    private async Task ProcessUserCreated(EventSourceModel evt)
    {
        // Simulate user creation processing (send welcome email, create profile, etc.)
        await Task.Delay(100);
        _logger.LogDebug($"User creation processed for event {evt.Id}");
    }

    private async Task ProcessUserUpdated(EventSourceModel evt)
    {
        // Simulate user update processing (update cache, sync data, etc.)
        await Task.Delay(75);
        _logger.LogDebug($"User update processed for event {evt.Id}");
    }

    private async Task ProcessUserDeleted(EventSourceModel evt)
    {
        // Simulate user deletion processing (cleanup data, audit logs, etc.)
        await Task.Delay(200);
        _logger.LogDebug($"User deletion processed for event {evt.Id}");
    }

    private async Task ProcessSystemMetric(EventSourceModel evt)
    {
        // Simulate metric processing (store in time-series DB, check thresholds, etc.)
        await Task.Delay(25);
        _logger.LogDebug($"System metric processed for event {evt.Id}");
    }

    private async Task ProcessSystemAlert(EventSourceModel evt)
    {
        // Simulate alert processing (send notifications, create tickets, etc.)
        await Task.Delay(150);
        _logger.LogDebug($"System alert processed for event {evt.Id}");
    }

    private async Task ProcessCriticalAlert(EventSourceModel evt)
    {
        // Simulate critical alert processing (immediate notifications, escalations, etc.)
        await Task.Delay(10);
        _logger.LogDebug($"Critical alert processed for event {evt.Id}");
    }

    // Lifecycle hooks
    public override Task ConsumingStarted()
    {
        _logger.LogInformation("🚀 Event Analytics Consumer STARTED at {Timestamp}", DateTime.UtcNow);
        return Task.CompletedTask;
    }

    public override Task ConsumingStopped()
    {
        _logger.LogWarning("⏹️ Event Analytics Consumer STOPPED at {Timestamp}", DateTime.UtcNow);
        return Task.CompletedTask;
    }

    public override Task ConsumingError(Exception exception)
    {
        _logger.LogError(exception, "💥 Error in Event Analytics Consumer");
        return Task.CompletedTask;
    }

    public override Task PartitionsAssigned(List<Confluent.Kafka.TopicPartition> partitions)
    {
        _logger.LogInformation("📍 Partitions assigned: {Partitions}", 
            string.Join(", ", partitions.Select(p => $"{p.Topic}:{p.Partition}")));
        return Task.CompletedTask;
    }

    public override Task PartitionsRevoked(List<Confluent.Kafka.TopicPartition> partitions)
    {
        _logger.LogInformation("📍 Partitions revoked: {Partitions}", 
            string.Join(", ", partitions.Select(p => $"{p.Topic}:{p.Partition}")));
        return Task.CompletedTask;
    }
}

/// <summary>
/// Event model matching the API producer
/// </summary>
public class EventSourceModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EventType { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Source { get; set; }
    public object Data { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}
