namespace QFace.Sdk.RabbitMq.Models;

[AttributeUsage(AttributeTargets.Method)]
public class TopicAttribute : Attribute
{
    public string ConfigurationKey { get; set; }
    public string ExchangeName { get; set;}
    public string RoutingKey { get; set; }
    public string QueueName { get; set; }
    public bool Durable { get; set; } = true;
    public bool AutoDelete { get; set; } = false;
    public int PrefetchCount { get; set; } = 10;

    // NEW: Constructor with required configurationKey
    public TopicAttribute(string configurationKey)
    {
        ConfigurationKey = configurationKey ?? throw new ArgumentNullException(nameof(configurationKey));
    }

    // OLD: Keep existing constructor for backward compatibility
    public TopicAttribute(string exchangeName, string queueName, string routingKey = "")
    {
        ExchangeName = exchangeName;
        RoutingKey = routingKey;
        QueueName = queueName ?? $"queue_{routingKey}";
        // ConfigurationKey will be null for old code - will use attribute values directly
    }

    // NEW: Constructor with configurationKey and optional values (for fallback support)
    public TopicAttribute(string configurationKey, string exchangeName, string queueName = "", string routingKey = "")
    {
        ConfigurationKey = configurationKey ?? throw new ArgumentNullException(nameof(configurationKey));
        ExchangeName = exchangeName;
        RoutingKey = routingKey;
        QueueName = queueName;
    }
}