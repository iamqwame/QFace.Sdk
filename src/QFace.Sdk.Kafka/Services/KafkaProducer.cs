using Newtonsoft.Json;
using QFace.Sdk.Kafka.Actors;

namespace QFace.Sdk.Kafka.Services;

/// <summary>
/// Kafka producer service implementation
/// </summary>
public class KafkaProducer : IKafkaProducer, IDisposable
{
    private readonly ILogger<KafkaProducer> _logger;
    private readonly IOptions<KafkaProducerConfig> _config;
    private readonly ActorSystem _actorSystem;
    private readonly IServiceProvider _serviceProvider;
    private IActorRef _producerActor;
    private readonly object _actorLock = new object();

    public KafkaProducer(ILogger<KafkaProducer> logger, 
        IOptions<KafkaProducerConfig> config,
        ActorSystem actorSystem,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _config = config;
        _actorSystem = actorSystem;
        _serviceProvider = serviceProvider;
    }
    
    private void EnsureProducerActor()
    {
        if (_producerActor != null) return;
        
        lock (_actorLock)
        {
            if (_producerActor != null) return;
            
            try
            {
                // Get the proper logger for the actor from DI
                var actorLogger = _serviceProvider.GetRequiredService<ILogger<KafkaProducerActor>>();
                
                // Create producer actor with correct logger type
                var props = Props.Create(() => new KafkaProducerActor(
                    actorLogger,
                    _config
                ));

                _producerActor = _actorSystem.ActorOf(props, "kafka-producer");
                _logger.LogInformation("[Kafka] Created producer actor successfully");
            }
            catch (InvalidActorNameException)
            {
                // Actor already exists, get reference to it
                _producerActor = _actorSystem.ActorSelection("user/kafka-producer").ResolveOne(TimeSpan.FromSeconds(5)).Result;
                _logger.LogInformation("[Kafka] Using existing producer actor");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Kafka] Failed to initialize producer actor");
                throw;
            }
        }
    }

    public async Task<DeliveryResult<string, string>> ProduceAsync<T>(string topic, T message, 
        string key = null, int? partition = null)
    {
        try
        {
            EnsureProducerActor();
            
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
        EnsureProducerActor();
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
