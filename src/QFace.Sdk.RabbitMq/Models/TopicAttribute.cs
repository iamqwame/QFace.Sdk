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

    // NEW: Constructor with required configurationKey only
    public TopicAttribute(string configurationKey)
    {
        ConfigurationKey = configurationKey ?? throw new ArgumentNullException(nameof(configurationKey));
    }

    // OLD: Keep existing constructor for backward compatibility (all parameters required to avoid ambiguity)
    public TopicAttribute(string exchangeName, string queueName, string routingKey)
    {
        ExchangeName = exchangeName;
        RoutingKey = routingKey;
        QueueName = queueName ?? $"queue_{routingKey}";
        // ConfigurationKey will be null for old code - will use attribute values directly
    }

    // OLD: Overload for backward compatibility (2 parameters, routingKey defaults to empty)
    public TopicAttribute(string exchangeName, string queueName)
        : this(exchangeName, queueName, "")
    {
    }

    // NEW: Constructor with configurationKey and optional values (for fallback support)
    // Note: This requires at least 3 parameters to avoid ambiguity with old constructor
    public TopicAttribute(string configurationKey, string exchangeName, string queueName, string routingKey)
    {
        ConfigurationKey = configurationKey ?? throw new ArgumentNullException(nameof(configurationKey));
        ExchangeName = exchangeName;
        RoutingKey = routingKey;
        QueueName = queueName;
    }
}