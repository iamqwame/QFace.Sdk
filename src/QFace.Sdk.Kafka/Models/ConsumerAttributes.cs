namespace QFace.Sdk.Kafka.Models;

/// <summary>
/// Attribute to mark a method as a Kafka topic consumer
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class ConsumeTopicAttribute : Attribute
{
    /// <summary>
    /// Named topic group from configuration (e.g., "Analytics", "Orders")
    /// </summary>
    public string TopicGroup { get; set; }
    
    /// <summary>
    /// Direct topic specification (overrides TopicGroup)
    /// </summary>
    public string[] DirectTopics { get; set; }
    
    /// <summary>
    /// Custom configuration path for topic resolution
    /// </summary>
    public string ConfigPath { get; set; }
    
    /// <summary>
    /// Whether this method processes messages in bulk or individually
    /// </summary>
    public bool IsBulk { get; set; } = true;
    
    /// <summary>
    /// Override batch size for this specific consumer method
    /// -1 means use global configuration
    /// </summary>
    public int MaxBatchSize { get; set; } = -1;
    
    /// <summary>
    /// Override batch timeout for this specific consumer method
    /// -1 means use global configuration
    /// </summary>
    public int BatchTimeoutMs { get; set; } = -1;
    
    /// <summary>
    /// Dead letter topic for failed messages
    /// </summary>
    public string DeadLetterTopic { get; set; }
    
    public ConsumeTopicAttribute(string topicGroup)
    {
        TopicGroup = topicGroup;
    }
    
    public ConsumeTopicAttribute(params string[] directTopics)
    {
        DirectTopics = directTopics;
    }
}

/// <summary>
/// Attribute to mark a class as a Kafka consumer
/// Similar to RabbitMQ's ConsumerAttribute but Kafka-specific
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class KafkaConsumerAttribute : Attribute
{
    public string Name { get; set; }
    public string GroupId { get; set; }
    
    public KafkaConsumerAttribute(string name = null, string groupId = null)
    {
        Name = name;
        GroupId = groupId;
    }
}
