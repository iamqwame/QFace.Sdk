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
        _logger.LogInformation(
            "Analytics batch received with {BatchSize} events at {Timestamp}",
            events.Count, DateTime.UtcNow);

        foreach (var evt in events)
        {
            _logger.LogInformation(
                "Processing event {EventId} of type {EventType} from {Source} with data {EventData}",
                evt.Id, evt.EventType, evt.Source, System.Text.Json.JsonSerializer.Serialize(evt.Data));

            // Simulate analytics processing
            await ProcessAnalyticsEvent(evt);
        }

        _logger.LogInformation("Analytics batch processed");
    }

    /// <summary>
    /// Handles user events using topic group
    /// </summary>
    [ConsumeTopic("UserEvents")]
    public async Task HandleUserEvents(List<EventSourceModel> events)
    {
        _logger.LogInformation("Received {UserEventCount} user events", events.Count);

        foreach (var evt in events)
        {
            switch (evt.EventType)
            {
                case "user.created":
                    _logger.LogInformation("User created event {EventId}", evt.Id);
                    await ProcessUserCreated(evt);
                    break;
                case "user.updated":
                    _logger.LogInformation("User updated event {EventId}", evt.Id);
                    await ProcessUserUpdated(evt);
                    break;
                case "user.deleted":
                    _logger.LogInformation("User deleted event {EventId}", evt.Id);
                    await ProcessUserDeleted(evt);
                    break;
                default:
                    _logger.LogInformation("Unknown user event type {EventType}", evt.EventType);
                    break;
            }
        }

        _logger.LogInformation("User events batch completed");
    }

    /// <summary>
    /// Handles system monitoring events
    /// </summary>
    [ConsumeTopic("SystemMonitoring")]
    public async Task HandleSystemEvents(List<EventSourceModel> events)
    {
        _logger.LogInformation("Processing {SystemEventCount} system events", events.Count);

        foreach (var evt in events)
        {
            if (evt.EventType == "system.metric")
            {
                _logger.LogInformation("Metric event {EventId}", evt.Id);
                await ProcessSystemMetric(evt);
            }
            else if (evt.EventType.StartsWith("system.alert"))
            {
                _logger.LogWarning("Alert event {EventId} of type {EventType}", evt.Id, evt.EventType);
                await ProcessSystemAlert(evt);
            }
        }

        _logger.LogInformation("System monitoring batch completed");
    }

    /// <summary>
    /// Handles critical alerts with direct topic specification (bypasses topic groups)
    /// </summary>
    [ConsumeTopic("alerts.critical", "alerts.emergency")]
    public async Task HandleCriticalAlerts(List<EventSourceModel> events)
    {
        _logger.LogCritical("Critical alerts received");

        foreach (var evt in events)
        {
            _logger.LogCritical(
                "Critical event {EventId} of type {EventType} with data {EventData}",
                evt.Id, evt.EventType, System.Text.Json.JsonSerializer.Serialize(evt.Data));

            // Process critical alerts immediately
            await ProcessCriticalAlert(evt);
        }

        _logger.LogCritical("Critical alerts processed");
    }

    private async Task ProcessAnalyticsEvent(EventSourceModel evt)
    {
        // Simulate analytics processing (store in data warehouse, update dashboards, etc.)
        await Task.Delay(5);
        _logger.LogDebug("Analytics processed for event {EventId}", evt.Id);
    }

    private async Task ProcessUserCreated(EventSourceModel evt)
    {
        // Simulate user creation processing (send welcome email, create profile, etc.)
        await Task.Delay(10);
        _logger.LogDebug("User creation processed for event {EventId}", evt.Id);
    }

    private async Task ProcessUserUpdated(EventSourceModel evt)
    {
        // Simulate user update processing (update cache, sync data, etc.)
        await Task.Delay(5);
        _logger.LogDebug("User update processed for event {EventId}", evt.Id);
    }

    private async Task ProcessUserDeleted(EventSourceModel evt)
    {
        // Simulate user deletion processing (cleanup data, audit logs, etc.)
        await Task.Delay(15);
        _logger.LogDebug("User deletion processed for event {EventId}", evt.Id);
    }

    private async Task ProcessSystemMetric(EventSourceModel evt)
    {
        // Simulate metric processing (store in time-series DB, check thresholds, etc.)
        await Task.Delay(2);
        _logger.LogDebug("System metric processed for event {EventId}", evt.Id);
    }

    private async Task ProcessSystemAlert(EventSourceModel evt)
    {
        // Simulate alert processing (send notifications, create tickets, etc.)
        await Task.Delay(8);
        _logger.LogDebug("System alert processed for event {EventId}", evt.Id);
    }

    private async Task ProcessCriticalAlert(EventSourceModel evt)
    {
        // Simulate critical alert processing (immediate notifications, escalations, etc.)
        await Task.Delay(1);
        _logger.LogDebug("Critical alert processed for event {EventId}", evt.Id);
    }

    // Lifecycle hooks
    public override Task ConsumingStarted()
    {
        _logger.LogInformation("Event Analytics Consumer started at {Timestamp}", DateTime.UtcNow);
        return Task.CompletedTask;
    }

    public override Task ConsumingStopped()
    {
        _logger.LogWarning("Event Analytics Consumer stopped at {Timestamp}", DateTime.UtcNow);
        return Task.CompletedTask;
    }

    public override Task ConsumingError(Exception exception)
    {
        _logger.LogError(exception, "Error in Event Analytics Consumer");
        return Task.CompletedTask;
    }

    public override Task PartitionsAssigned(List<Confluent.Kafka.TopicPartition> partitions)
    {
        _logger.LogInformation("Partitions assigned: {Partitions}",
            string.Join(", ", partitions.Select(p => $"{p.Topic}:{p.Partition}")));
        return Task.CompletedTask;
    }

    public override Task PartitionsRevoked(List<Confluent.Kafka.TopicPartition> partitions)
    {
        _logger.LogInformation("Partitions revoked: {Partitions}",
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
    public string EventType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Source { get; set; } = string.Empty;
    public object? Data { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}
