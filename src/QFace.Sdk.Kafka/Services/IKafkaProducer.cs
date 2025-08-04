namespace QFace.Sdk.Kafka.Services;

/// <summary>
/// Interface for Kafka message production
/// </summary>
public interface IKafkaProducer
{
    /// <summary>
    /// Produce a message to a Kafka topic
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="topic">Target topic</param>
    /// <param name="message">Message to send</param>
    /// <param name="key">Optional message key for partitioning</param>
    /// <param name="partition">Optional specific partition</param>
    /// <returns>Produce result with metadata</returns>
    Task<DeliveryResult<string, string>> ProduceAsync<T>(string topic, T message, 
        string key = null, int? partition = null);
    
    /// <summary>
    /// Produce a message with a key selector function
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="topic">Target topic</param>
    /// <param name="message">Message to send</param>
    /// <param name="keySelector">Function to extract key from message</param>
    /// <returns>Produce result with metadata</returns>
    Task<DeliveryResult<string, string>> ProduceAsync<T>(string topic, T message, 
        Func<T, string> keySelector);
    
    /// <summary>
    /// Produce multiple messages in a batch
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="topic">Target topic</param>
    /// <param name="messages">Messages to send</param>
    /// <param name="keySelector">Optional function to extract key from each message</param>
    /// <returns>Collection of produce results</returns>
    Task<IEnumerable<DeliveryResult<string, string>>> ProduceBatchAsync<T>(string topic, 
        IEnumerable<T> messages, Func<T, string> keySelector = null);
}
