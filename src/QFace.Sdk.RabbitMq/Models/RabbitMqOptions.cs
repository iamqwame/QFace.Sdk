namespace QFace.Sdk.RabbitMq.Models;
public class RabbitMqOptions
{
    public string ConnectionString { get; set; }
    public string Title { get; set; }
    public string ExchangeType { get; set; } =RabbitMQ.Client.ExchangeType.Fanout;
        
    // Add option to passively check exchange (not try to create/modify it)
    // WARNING: Only set to true if exchanges are pre-created by RabbitMQ admins
    public bool PassiveExchange { get; set; } = false;
    public bool AutomaticRecoveryEnabled { get; set; } = true;
    public int RetryCount { get; set; } = 5;
    public int RetryIntervalMs { get; set; } = 2000; // Base delay for exponential backoff
    
    // Timeout settings
    public int RequestedConnectionTimeout { get; set; } = 30000; // 30 seconds
    public ushort RequestedHeartbeat { get; set; } = 60; // 60 seconds
    public int ContinuationTimeout { get; set; } = 10000; // 10 seconds
    public int SocketReadTimeout { get; set; } = 30000; // 30 seconds
    
    public int PublisherInstances { get; set; } = 10;
    public int PublisherUpperBound { get; set; } = 100;
    public int ConsumerInstances { get; set; } = 10;
    public int ConsumerUpperBound { get; set; } = 100;
}