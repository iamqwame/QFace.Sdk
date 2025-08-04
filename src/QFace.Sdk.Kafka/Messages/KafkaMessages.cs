namespace QFace.Sdk.Kafka.Messages;

/// <summary>
/// Message sent to Kafka producer actor
/// </summary>
internal class ProduceMessage
{
    public object Message { get; }
    public string Topic { get; }
    public string Key { get; }
    public int? Partition { get; }
    public Type MessageType { get; }

    public ProduceMessage(object message, string topic, string key = null, int? partition = null)
    {
        Message = message;
        Topic = topic;
        Key = key;
        Partition = partition;
        MessageType = message.GetType();
    }
}

/// <summary>
/// Message sent to Kafka consumer actor for batch processing
/// </summary>
internal class ConsumeMessage
{
    public List<object> Messages { get; }
    public string TopicGroup { get; }
    public Dictionary<string, object> Metadata { get; }

    public ConsumeMessage(List<object> messages, string topicGroup, Dictionary<string, object> metadata = null)
    {
        Messages = messages;
        TopicGroup = topicGroup;
        Metadata = metadata ?? new Dictionary<string, object>();
    }
}

/// <summary>
/// Message to start consuming from specific topics
/// </summary>
internal class StartConsumingMessage
{
    public List<string> Topics { get; }
    public string ConsumerGroupId { get; }

    public StartConsumingMessage(List<string> topics, string consumerGroupId)
    {
        Topics = topics;
        ConsumerGroupId = consumerGroupId;
    }
}

/// <summary>
/// Message to stop consuming
/// </summary>
internal class StopConsumingMessage
{
    public static readonly StopConsumingMessage Instance = new();
}
