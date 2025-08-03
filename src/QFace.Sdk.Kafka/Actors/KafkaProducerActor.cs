using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using QFace.Sdk.ActorSystems;
using QFace.Sdk.Kafka.Messages;
using QFace.Sdk.Kafka.Models;

namespace QFace.Sdk.Kafka.Actors;

/// <summary>
/// Actor responsible for producing messages to Kafka topics
/// </summary>
internal class KafkaProducerActor : BaseActor
{
    private readonly ILogger<KafkaProducerActor> _logger;
    private readonly KafkaProducerConfig _config;
    private IProducer<string, string> _producer;
    private readonly object _producerLock = new object();

    public KafkaProducerActor(
        ILogger<KafkaProducerActor> logger,
        IOptions<KafkaProducerConfig> config)
    {
        _logger = logger;
        _config = config.Value;
        
        Initialize();
        
        ReceiveAsync<ProduceMessage>(HandleProduceMessage);
    }

    private void Initialize()
    {
        try
        {
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = _config.BootstrapServers,
                Acks = Enum.Parse<Acks>(_config.Acks),
                //Retries = _config.Retries,
                BatchSize = _config.BatchSize,
                LingerMs = _config.LingerMs,
                CompressionType = Enum.Parse<CompressionType>(_config.CompressionType),
                EnableIdempotence = true, // Prevent duplicate messages
                MaxInFlight = 5
            };
            
            // Apply extra properties
            foreach (var prop in _config.ExtraProperties)
            {
                producerConfig.Set(prop.Key, prop.Value);
            }

            _producer = new ProducerBuilder<string, string>(producerConfig)
                .SetErrorHandler((_, e) => 
                    _logger.LogError($"[Kafka] Producer error: {e.Reason} - {e.Code}"))
                .SetStatisticsHandler((_, json) => 
                    _logger.LogDebug($"[Kafka] Producer statistics: {json}"))
                .Build();
                
            _logger.LogInformation("[Kafka] Producer actor initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Kafka] Failed to initialize producer actor");
            throw;
        }
    }

    private async Task HandleProduceMessage(ProduceMessage message)
    {
        try
        {
            var result = await ProduceWithRetryAsync(message);
            Sender.Tell(result);
        }
        catch (Exception ex)
        {
            Sender.Tell(new Status.Failure(ex));
        }
    }

    private async Task<DeliveryResult<string, string>> ProduceWithRetryAsync(ProduceMessage message, int currentRetry = 0)
    {
        try
        {
            lock (_producerLock)
            {
                if (_producer == null)
                {
                    _logger.LogWarning("[Kafka] Producer is null. Reinitializing...");
                    Initialize();
                }
            }

            var serializedMessage = JsonConvert.SerializeObject(
                message.Message,
                Formatting.None,
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }
            );

            var kafkaMessage = new Message<string, string>
            {
                Key = message.Key,
                Value = serializedMessage,
                Headers = new Headers
                {
                    { "MessageType", System.Text.Encoding.UTF8.GetBytes(message.MessageType.FullName) },
                    { "ProducedAt", System.Text.Encoding.UTF8.GetBytes(DateTimeOffset.UtcNow.ToString("O")) },
                    { "ProducerId", System.Text.Encoding.UTF8.GetBytes(Environment.MachineName) }
                }
            };

            DeliveryResult<string, string> result;
            
            if (message.Partition.HasValue)
            {
                var topicPartition = new TopicPartition(message.Topic, message.Partition.Value);
                result = await _producer.ProduceAsync(topicPartition, kafkaMessage);
            }
            else
            {
                result = await _producer.ProduceAsync(message.Topic, kafkaMessage);
            }

            _logger.LogInformation(
                $"[Kafka] ✅ Produced message to {result.Topic}:{result.Partition}:{result.Offset} " +
                $"(Key: {message.Key ?? "null"})");
                
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[Kafka] ❌ Failed to produce message to topic {message.Topic}. Error: {ex.Message}");
            
            if (currentRetry < 3) // TODO: Make configurable
            {
                _logger.LogInformation($"[Kafka] Retrying produce ({currentRetry + 1}/3)...");
                await Task.Delay(1000 * (currentRetry + 1)); // Exponential backoff
                return await ProduceWithRetryAsync(message, currentRetry + 1);
            }
            
            throw;
        }
    }

    protected override void PostStop()
    {
        lock (_producerLock)
        {
            try
            {
                _producer?.Flush(TimeSpan.FromSeconds(10));
                _producer?.Dispose();
                _logger.LogInformation("[Kafka] Producer actor stopped and disposed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Kafka] Error disposing producer");
            }
        }
        
        base.PostStop();
    }
}
