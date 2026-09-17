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
