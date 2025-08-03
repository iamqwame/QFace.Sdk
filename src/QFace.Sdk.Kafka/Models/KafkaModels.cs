namespace QFace.Sdk.Kafka.Models;

/// <summary>
/// Result of message processing for offset management
/// </summary>
public enum ConsumeResult
{
    Success,
    Retry,
    Skip,
    DeadLetter
}

/// <summary>
/// Provides access to top-level actors from the ActorSystems SDK
/// </summary>
public interface ITopLevelActors
{
    IActorRef GetActor<T>(string name = "") where T : BaseActor;
}

/// <summary>
/// Implementation of ITopLevelActors that wraps the static TopLevelActors class
/// </summary>
public class TopLevelActorsWrapper : ITopLevelActors
{
    public IActorRef GetActor<T>(string name = "") where T : BaseActor
    {
        return TopLevelActors.GetActor<T>(name);
    }
}

/// <summary>
/// Provides access to Kafka-specific context during message processing
/// </summary>
public class KafkaConsumerContext
{
    /// <summary>
    /// Current batch being processed with partition and offset information
    /// </summary>
    public KafkaBatch CurrentBatch { get; internal set; }
    
    /// <summary>
    /// Consumer instance for manual operations
    /// </summary>
    public IConsumer<string, string> Consumer { get; internal set; }
    
    /// <summary>
    /// Manually commit offsets (only available when manual offset management is enabled)
    /// </summary>
    public async Task CommitAsync()
    {
        if (Consumer != null)
        {
            await Task.Run(() => Consumer.Commit());
        }
    }
    
    /// <summary>
    /// Commit specific offsets
    /// </summary>
    public async Task CommitAsync(IEnumerable<TopicPartitionOffset> offsets)
    {
        if (Consumer != null)
        {
            await Task.Run(() => Consumer.Commit(offsets));
        }
    }
}

/// <summary>
/// Represents a batch of Kafka messages with metadata
/// </summary>
public class KafkaBatch
{
    public List<KafkaMessage> Messages { get; set; } = new();
    public Dictionary<TopicPartition, Offset> PartitionOffsets { get; set; } = new();
    public DateTime BatchStartTime { get; set; }
    public DateTime BatchEndTime { get; set; }
    public string TopicGroup { get; set; }
}

/// <summary>
/// Individual Kafka message with full metadata
/// </summary>
public class KafkaMessage
{
    public string Topic { get; set; }
    public int Partition { get; set; }
    public long Offset { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
    public DateTime Timestamp { get; set; }
    public Headers Headers { get; set; }
}

/// <summary>
/// Strongly typed Kafka message
/// </summary>
public class KafkaMessage<T> : KafkaMessage
{
    public T TypedValue { get; set; }
}
