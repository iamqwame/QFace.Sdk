namespace QFace.Sdk.Kafka.Models;

public class KafkaConsumerConfig
{
    public string BootstrapServers { get; set; } = "localhost:9092";
    public string GroupId { get; set; } = "default-group";
    
    /// <summary>
    /// Named groups of topics for logical organization
    /// Example: "Analytics": ["user.events", "system.events"]
    /// </summary>
    public Dictionary<string, List<string>> TopicGroups { get; set; } = new();
    
    /// <summary>
    /// Direct topic list for backward compatibility
    /// </summary>
    public List<string> Topics { get; set; } = new();
    
    /// <summary>
    /// Additional Kafka consumer properties
    /// </summary>
    public Dictionary<string, string> ExtraProperties { get; set; } = new();
    
    /// <summary>
    /// Batch processing configuration
    /// </summary>
    public int MaxBatchSize { get; set; } = 200;
    public int BatchTimeoutMs { get; set; } = 5000;
    
    /// <summary>
    /// Consumer behavior settings
    /// </summary>
    public bool EnableAutoCommit { get; set; } = false; // Manual commit for better control
    public int SessionTimeoutMs { get; set; } = 30000;
    public int MaxPollIntervalMs { get; set; } = 300000;
    
    /// <summary>
    /// Error handling
    /// </summary>
    public int RetryCount { get; set; } = 3;
    public int RetryIntervalMs { get; set; } = 1000;
}

public class KafkaProducerConfig
{
    public string BootstrapServers { get; set; } = "localhost:9092";
    
    /// <summary>
    /// Additional Kafka producer properties
    /// </summary>
    public Dictionary<string, string> ExtraProperties { get; set; } = new();
    
    /// <summary>
    /// Producer behavior settings
    /// </summary>
    public string Acks { get; set; } = "1"; // Wait for leader acknowledgment
    public int Retries { get; set; } = 3;
    public int BatchSize { get; set; } = 16384;
    public int LingerMs { get; set; } = 5;
    public string CompressionType { get; set; } = "None";
    
    /// <summary>
    /// Actor pool settings
    /// </summary>
    public int ProducerInstances { get; set; } = 10;
    public int ProducerUpperBound { get; set; } = 100;
}

public class MessageGroupConsumerLogicConfig
{
    public int TimeoutInMilliseconds { get; set; } = 5000;
    public int MaxElements { get; set; } = 200;
    
    /// <summary>
    /// How to handle offset commits
    /// </summary>
    public OffsetCommitStrategy CommitStrategy { get; set; } = OffsetCommitStrategy.AfterSuccessfulProcessing;
    
    /// <summary>
    /// Enable manual offset management for advanced scenarios
    /// </summary>
    public bool EnableManualOffsetManagement { get; set; } = false;
}

public enum OffsetCommitStrategy
{
    /// <summary>
    /// Commit offsets only after successful message processing
    /// Provides at-least-once delivery guarantee
    /// </summary>
    AfterSuccessfulProcessing,
    
    /// <summary>
    /// Commit offsets immediately after receiving messages
    /// Provides at-most-once delivery guarantee
    /// </summary>
    AfterBatchReceived,
    
    /// <summary>
    /// Developer controls offset commits manually via context
    /// </summary>
    Manual
}
