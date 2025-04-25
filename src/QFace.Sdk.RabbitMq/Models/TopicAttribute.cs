namespace QFace.Sdk.RabbitMq.Models;

[AttributeUsage(AttributeTargets.Method)]
public class TopicAttribute : Attribute
{
    public string ExchangeName { get; set;}
    public string RoutingKey { get; set; }
    public string QueueName { get; set; }
    public bool Durable { get; set; } = true;
    public bool AutoDelete { get; set; } = false;
    public int PrefetchCount { get; set; } = 10;

    public TopicAttribute(string exchangeName, string queueName,string routingKey="")
    {
        ExchangeName = exchangeName;
        RoutingKey = routingKey;
        QueueName = queueName ?? $"queue_{routingKey}";
    }
}