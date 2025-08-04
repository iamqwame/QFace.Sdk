using Microsoft.Extensions.Options;
using QFace.Sdk.ActorSystems;
using QFace.Sdk.Kafka.Consumer;
using QFace.Sdk.Kafka.Messages;
using QFace.Sdk.Kafka.Models;

namespace QFace.Sdk.Kafka.Actors;

/// <summary>
/// Supervisor actor that manages multiple Kafka consumer actors
/// Handles consumer lifecycle, restarts, and coordination
/// </summary>
internal class KafkaConsumerSupervisorActor : BaseActor
{
    private readonly ILogger<KafkaConsumerSupervisorActor> _logger;
    private readonly List<ConsumerMetadata> _consumerMetadata;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, IActorRef> _consumerActors = new();

    public KafkaConsumerSupervisorActor(
        ILogger<KafkaConsumerSupervisorActor> logger,
        List<ConsumerMetadata> consumerMetadata,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _consumerMetadata = consumerMetadata;
        _serviceProvider = serviceProvider;
        
        InitializeConsumers();
        
        ReceiveAsync<StartConsumingMessage>(HandleStartConsuming);
        ReceiveAsync<StopConsumingMessage>(HandleStopConsuming);
    }

    private void InitializeConsumers()
    {
        try
        {
            foreach (var metadata in _consumerMetadata)
            {
                var consumerActorName = $"kafka-consumer-{metadata.ConsumerType.Name}-{metadata.HandlerMethod.Name}";
                
                var consumerActorProps = Props.Create(() => new KafkaConsumerActor(
                    _serviceProvider.GetRequiredService<ILogger<KafkaConsumerActor>>(),
                    metadata,
                    _serviceProvider.GetRequiredService<IOptions<KafkaConsumerConfig>>(),
                    _serviceProvider.GetRequiredService<IOptions<MessageGroupConsumerLogicConfig>>(),
                    _serviceProvider
                ));

                var consumerActor = Context.ActorOf(consumerActorProps, consumerActorName);
                _consumerActors[consumerActorName] = consumerActor;
                
                _logger.LogInformation($"[Kafka] Created consumer actor: {consumerActorName}");
                
                // Start consuming immediately
                var startMessage = new StartConsumingMessage(metadata.Topics, metadata.ConsumerGroupId ?? _serviceProvider.GetRequiredService<IOptions<KafkaConsumerConfig>>().Value.GroupId);
                consumerActor.Tell(startMessage);
            }
            
            _logger.LogInformation($"[Kafka] Supervisor initialized {_consumerActors.Count} consumer actors");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Kafka] Failed to initialize consumer actors");
            throw;
        }
    }

    private async Task HandleStartConsuming(StartConsumingMessage message)
    {
        try
        {
            foreach (var consumerActor in _consumerActors.Values)
            {
                consumerActor.Tell(message);
            }
            
            _logger.LogInformation("[Kafka] Started all consumer actors");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Kafka] Failed to start consumer actors");
        }
    }

    private async Task HandleStopConsuming(StopConsumingMessage message)
    {
        try
        {
            var stopTasks = _consumerActors.Values.Select(actor => 
            {
                actor.Tell(message);
                return actor.GracefulStop(TimeSpan.FromSeconds(30));
            });
            
            await Task.WhenAll(stopTasks);
            
            _logger.LogInformation("[Kafka] Stopped all consumer actors");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Kafka] Failed to stop consumer actors gracefully");
        }
    }

    protected override SupervisorStrategy SupervisorStrategy()
    {
        return new OneForOneStrategy(
            maxNrOfRetries: 3,
            withinTimeRange: TimeSpan.FromMinutes(1),
            localOnlyDecider: ex =>
            {
                _logger.LogWarning(ex, "[Kafka] Consumer actor failed, restarting...");
                
                return ex switch
                {
                    Confluent.Kafka.ConsumeException => Directive.Restart,
                    InvalidOperationException => Directive.Restart,
                    _ => Directive.Escalate
                };
            });
    }

    protected override void PostStop()
    {
        try
        {
            foreach (var actor in _consumerActors.Values)
            {
                actor.Tell(PoisonPill.Instance);
            }
            
            _logger.LogInformation("[Kafka] Supervisor actor stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Kafka] Error stopping supervisor");
        }
        
        base.PostStop();
    }
}
