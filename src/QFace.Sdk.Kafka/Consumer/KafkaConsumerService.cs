namespace QFace.Sdk.Kafka.Consumer;

/// <summary>
/// Hosted service that manages Kafka consumer lifecycle
/// </summary>
public class KafkaConsumerService : IHostedService
{
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly List<ConsumerMetadata> _consumerMetadata;
    private readonly IServiceProvider _serviceProvider;
    private readonly ActorSystem _actorSystem;
    private IActorRef _supervisorActor;

    public KafkaConsumerService(
        ILogger<KafkaConsumerService> logger,
        List<ConsumerMetadata> consumerMetadata,
        IServiceProvider serviceProvider,
        ActorSystem actorSystem)
    {
        _logger = logger;
        _consumerMetadata = consumerMetadata;
        _serviceProvider = serviceProvider;
        _actorSystem = actorSystem;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (!_consumerMetadata.Any())
            {
                _logger.LogInformation("[Kafka] No consumers found, service will not start");
                return;
            }

            _logger.LogInformation($"[Kafka] Starting consumer service with {_consumerMetadata.Count} consumers");

            // Create supervisor actor
            var supervisorProps = Props.Create(() => new Actors.KafkaConsumerSupervisorActor(
                _serviceProvider.GetRequiredService<ILogger<Actors.KafkaConsumerSupervisorActor>>(),
                _consumerMetadata,
                _serviceProvider
            ));

            _supervisorActor = _actorSystem.ActorOf(supervisorProps, "kafka-consumer-supervisor");

            _logger.LogInformation("[Kafka] Consumer service started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Kafka] Failed to start consumer service");
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_supervisorActor != null)
            {
                _logger.LogInformation("[Kafka] Stopping consumer service");
                
                // Gracefully stop the supervisor (which will stop all child consumers)
                await _supervisorActor.GracefulStop(TimeSpan.FromSeconds(30));
                
                _logger.LogInformation("[Kafka] Consumer service stopped successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Kafka] Error stopping consumer service");
        }
    }
}
