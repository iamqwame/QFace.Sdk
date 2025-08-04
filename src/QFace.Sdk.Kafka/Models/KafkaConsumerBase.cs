using Confluent.Kafka;

namespace QFace.Sdk.Kafka.Models;

/// <summary>
/// Base class for all Kafka consumers using the QFace actor-based approach
/// </summary>
public abstract class KafkaConsumerBase
{
    protected ILogger Logger { get; private set; }
    protected ITopLevelActors TopLevelActors { get; private set; }
    protected KafkaConsumerContext Context { get; private set; }
    
    /// <summary>
    /// Internal initialization called by the framework
    /// </summary>
    internal void Initialize(ILogger logger, ITopLevelActors topLevelActors, KafkaConsumerContext context)
    {
        Logger = logger;
        TopLevelActors = topLevelActors;
        Context = context;
    }
    
    /// <summary>
    /// Called when the consumer starts consuming messages
    /// </summary>
    public virtual Task ConsumingStarted() => Task.CompletedTask;
    
    /// <summary>
    /// Called when the consumer stops consuming messages
    /// </summary>
    public virtual Task ConsumingStopped() => Task.CompletedTask;
    
    /// <summary>
    /// Called when an error occurs during consumption
    /// </summary>
    public virtual Task ConsumingError(Exception exception) => Task.CompletedTask;
    
    /// <summary>
    /// Called when partitions are assigned to this consumer
    /// </summary>
    public virtual Task PartitionsAssigned(List<TopicPartition> partitions) => Task.CompletedTask;
    
    /// <summary>
    /// Called when partitions are revoked from this consumer
    /// </summary>
    public virtual Task PartitionsRevoked(List<TopicPartition> partitions) => Task.CompletedTask;
}
