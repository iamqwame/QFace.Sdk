namespace QFace.Sdk.Kafka.Examples;

/// <summary>
/// Example Kafka consumer following your desired pattern
/// </summary>
public class GovAnalyticsConsumer : KafkaConsumerBase
{
    private readonly ILogger<GovAnalyticsConsumer> _logger;

    public GovAnalyticsConsumer(ILogger<GovAnalyticsConsumer> logger)
    {
        _logger = logger;
        // Note: topLevelActors will be injected via Initialize() method
    }

    /// <summary>
    /// Handles bulk processing of government analytics events
    /// Uses the "GovAnalytics" topic group from configuration
    /// </summary>
    [ConsumeTopic("GovAnalytics")]
    public async Task HandleBulkMessage(List<EventSourceModel> messages)
    {
        try
        {
            _logger.LogInformation($"Processing batch of {messages.Count} government analytics events");
            
            // Your business logic here
            foreach (var message in messages)
            {
                // Process individual event
                _logger.LogDebug($"Processing event: {message.Id} of type {message.EventType}");
                
                // Example: Send to analytics pipeline
                // await _analyticsService.ProcessEvent(message);
            }
            
            _logger.LogInformation($"Successfully processed {messages.Count} events");
            
            await Task.CompletedTask;
        }
        catch (Exception e)
        {
            _logger.LogError($"Error processing bulk messages: {e.Message}");
            throw; // Let the framework handle retry logic
        }
    }

    /// <summary>
    /// Alternative: Handle specific topics directly (bypasses topic groups)
    /// </summary>
    [ConsumeTopic("audit.events", "compliance.events")]
    public async Task HandleAuditEvents(List<AuditEventModel> messages)
    {
        try
        {
            _logger.LogInformation($"Processing {messages.Count} audit events");
            
            // Audit-specific processing
            foreach (var auditEvent in messages)
            {
                // Handle audit logic
            }
            
            await Task.CompletedTask;
        }
        catch (Exception e)
        {
            _logger.LogError($"Error processing audit events: {e.Message}");
            throw;
        }
    }

    public override Task ConsumingStarted()
    {
        _logger.LogInformation($"Government analytics consumer started at {DateTime.UtcNow}");
        return Task.CompletedTask;
    }

    public override Task ConsumingStopped()
    {
        _logger.LogWarning($"Government analytics consumer stopped at {DateTime.UtcNow}");
        return Task.CompletedTask;
    }

    public override Task ConsumingError(Exception exception)
    {
        _logger.LogError(exception, "Error in government analytics consumer");
        return Task.CompletedTask;
    }

    public override Task PartitionsAssigned(List<TopicPartition> partitions)
    {
        _logger.LogInformation($"Partitions assigned: {string.Join(", ", partitions.Select(p => $"{p.Topic}:{p.Partition}"))}");
        return Task.CompletedTask;
    }

    public override Task PartitionsRevoked(List<TopicPartition> partitions)
    {
        _logger.LogInformation($"Partitions revoked: {string.Join(", ", partitions.Select(p => $"{p.Topic}:{p.Partition}"))}");
        return Task.CompletedTask;
    }
}

/// <summary>
/// Example event model for government analytics
/// </summary>
public class EventSourceModel
{
    public string Id { get; set; }
    public string EventType { get; set; }
    public DateTime Timestamp { get; set; }
    public string Source { get; set; }
    public object Data { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Example audit event model
/// </summary>
public class AuditEventModel
{
    public string EventId { get; set; }
    public string UserId { get; set; }
    public string Action { get; set; }
    public string Resource { get; set; }
    public DateTime Timestamp { get; set; }
    public string IpAddress { get; set; }
    public Dictionary<string, object> Context { get; set; } = new();
}
