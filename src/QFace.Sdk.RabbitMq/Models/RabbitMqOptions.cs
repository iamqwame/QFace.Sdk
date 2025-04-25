namespace QFace.Sdk.RabbitMq.Models;
public class RabbitMqOptions
{
    public string ConnectionString { get; set; }
    public string Title { get; set; }
    public string ExchangeType { get; set; } =RabbitMQ.Client.ExchangeType.Fanout;
        
    // Add option to passively check exchange (not try to create/modify it)
    public bool PassiveExchange { get; set; } = true;
    public bool AutomaticRecoveryEnabled { get; set; } = true;
    public int RetryCount { get; set; } = 5;
    public int RetryIntervalMs { get; set; } = 1000;
    public int PublisherInstances { get; set; } = 10;
    public int PublisherUpperBound { get; set; } = 100;
    public int ConsumerInstances { get; set; } = 10;
    public int ConsumerUpperBound { get; set; } = 100;
}