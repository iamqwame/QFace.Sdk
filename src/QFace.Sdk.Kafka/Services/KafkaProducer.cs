using Newtonsoft.Json;
using QFace.Sdk.Kafka.Actors;

namespace QFace.Sdk.Kafka.Services;

/// <summary>
/// Kafka producer service implementation that uses the actor system
/// </summary>
public class KafkaProducer : IKafkaProducer, IDisposable
{
    private readonly ILogger<KafkaProducer> _logger;
    private readonly ITopLevelActors _topLevelActors;
    private readonly ActorSystem _actorSystem;
    private IActorRef _producerActor;

    public KafkaProducer(ILogger<KafkaProducer> logger, 
        ITopLevelActors topLevelActors,
        ActorSystem actorSystem)
    {
        _logger = logger;
        _topLevelActors = topLevelActors;
        _actorSystem = actorSystem;
        
        InitializeProducerActor();
    }
    
    private void InitializeProducerActor()
    {
        try
        {
            // Try to get existing actor first
            _producerActor = _topLevelActors.GetActor<KafkaProducerActor>("_KafkaProducerActor");
            _logger.LogDebug("[Kafka] Using existing producer actor");
        }
        catch (ArgumentOutOfRangeException)
        {
            // Actor doesn't exist, this is expected in some scenarios
            _logger.LogDebug("[Kafka] Producer actor not found in TopLevelActors, will use direct actor reference");
            
            // Try to find the actor by name in the actor system
            _producerActor = _actorSystem.ActorSelection("user/kafka-producer").ResolveOne(TimeSpan.FromSeconds(1)).Result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Kafka] Failed to initialize producer actor reference");
            throw;
        }
    }

    public async Task<DeliveryResult<string, string>> ProduceAsync<T>(string topic, T message, 
        string key = null, int? partition = null)
    {
        try
        {
            var produceMessage = new ProduceMessage(message, topic, key, partition);
            
            // Send message to producer actor and wait for result
            var result = await _producerActor.Ask<DeliveryResult<string, string>>(produceMessage, TimeSpan.FromSeconds(30));
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[Kafka] ❌ Failed to produce message to topic {topic}");
            throw;
        }
    }

    public async Task<DeliveryResult<string, string>> ProduceAsync<T>(string topic, T message, 
        Func<T, string> keySelector)
    {
        var key = keySelector?.Invoke(message);
        return await ProduceAsync(topic, message, key);
    }

    public async Task<IEnumerable<DeliveryResult<string, string>>> ProduceBatchAsync<T>(string topic, 
        IEnumerable<T> messages, Func<T, string> keySelector = null)
    {
        var results = new List<DeliveryResult<string, string>>();
        
        // Process messages in parallel for better performance
        var tasks = messages.Select(async message =>
        {
            var key = keySelector?.Invoke(message);
            return await ProduceAsync(topic, message, key);
        });
        
        var completedResults = await Task.WhenAll(tasks);
        return completedResults;
    }

    public void Dispose()
    {
        // Actor system handles cleanup
        // No direct disposal needed as actors manage their own resources
    }
}
