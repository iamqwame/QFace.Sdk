using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using QFace.Sdk.ActorSystems;
using QFace.Sdk.Kafka.Consumer;
using QFace.Sdk.Kafka.Messages;
using QFace.Sdk.Kafka.Models;
using System.Linq;

namespace QFace.Sdk.Kafka.Actors;

/// <summary>
/// Actor responsible for consuming messages from Kafka topics and processing them in batches
/// </summary>
internal class KafkaConsumerActor : BaseActor
{
    private readonly ILogger<KafkaConsumerActor> _logger;
    private readonly ConsumerMetadata _metadata;
    private readonly KafkaConsumerConfig _consumerConfig;
    private readonly MessageGroupConsumerLogicConfig _processingConfig;
    private readonly IServiceProvider _serviceProvider;
    
    private IConsumer<string, string> _consumer;
    private KafkaConsumerBase _consumerInstance;
    private KafkaConsumerContext _context;
    private CancellationTokenSource _cancellationTokenSource;
    private Task _consumingTask;
    private readonly object _consumerLock = new object();

    public KafkaConsumerActor(
        ILogger<KafkaConsumerActor> logger,
        ConsumerMetadata metadata,
        IOptions<KafkaConsumerConfig> consumerConfig,
        IOptions<MessageGroupConsumerLogicConfig> processingConfig,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _metadata = metadata;
        _consumerConfig = consumerConfig.Value;
        _processingConfig = processingConfig.Value;
        _serviceProvider = serviceProvider;
        
        Initialize();
        
        ReceiveAsync<StartConsumingMessage>(HandleStartConsuming);
        ReceiveAsync<StopConsumingMessage>(HandleStopConsuming);
    }

    private void Initialize()
    {
        try
        {
            // Create consumer configuration
            var config = new ConsumerConfig
            {
                BootstrapServers = _consumerConfig.BootstrapServers,
                GroupId = _metadata.ConsumerGroupId ?? _consumerConfig.GroupId,
                EnableAutoCommit = _consumerConfig.EnableAutoCommit,
                SessionTimeoutMs = _consumerConfig.SessionTimeoutMs,
                MaxPollIntervalMs = _consumerConfig.MaxPollIntervalMs,
                AutoOffsetReset = AutoOffsetReset.Latest
            };
            
            // Apply extra properties
            foreach (var prop in _consumerConfig.ExtraProperties)
            {
                config.Set(prop.Key, prop.Value);
            }
            
            // Enable auto topic creation by default for consumers too
            config.Set("allow.auto.create.topics", "true");

            _consumer = new ConsumerBuilder<string, string>(config)
                .SetErrorHandler((_, e) => 
                    _logger.LogError($"[Kafka] Consumer error: {e.Reason} - {e.Code}"))
                .SetPartitionsAssignedHandler((_, partitions) =>
                {
                    _logger.LogInformation($"[Kafka] Partitions assigned: {string.Join(", ", partitions)}");
                    Task.Run(async () =>
                    {
                        if (_consumerInstance != null)
                        {
                            await _consumerInstance.PartitionsAssigned(partitions);
                        }
                    });
                })
                .SetPartitionsRevokedHandler((_, partitions) =>
                {
                    _logger.LogInformation($"[Kafka] Partitions revoked: {string.Join(", ", partitions)}");
                    Task.Run(async () =>
                    {
                        if (_consumerInstance != null)
                        {
                            // Convert TopicPartitionOffset to TopicPartition
                            var topicPartitions = partitions.Select(p => p.TopicPartition).ToList();
                            await _consumerInstance.PartitionsRevoked(topicPartitions);
                        }
                    });
                })
                .Build();

            // Create consumer instance
            _consumerInstance = (KafkaConsumerBase)_serviceProvider.GetRequiredService(_metadata.ConsumerType);
            
            // Initialize context
            _context = new KafkaConsumerContext
            {
                Consumer = _consumer
            };
            
            // Initialize the consumer instance
            var topLevelActors = _serviceProvider.GetRequiredService<ITopLevelActors>();
            _consumerInstance.Initialize(_logger, topLevelActors, _context);
            
            _logger.LogInformation($"[Kafka] Consumer actor initialized for {_metadata.ConsumerType.Name}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Kafka] Failed to initialize consumer actor");
            throw;
        }
    }

    private async Task HandleStartConsuming(StartConsumingMessage message)
    {
        try
        {
            if (_consumingTask != null && !_consumingTask.IsCompleted)
            {
                _logger.LogWarning("[Kafka] Consumer is already running");
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            
            lock (_consumerLock)
            {
                _consumer.Subscribe(message.Topics);
            }
            
            await _consumerInstance.ConsumingStarted();
            
            _consumingTask = Task.Run(async () => await ConsumeLoop(_cancellationTokenSource.Token));
            
            _logger.LogInformation($"[Kafka] Started consuming from topics: {string.Join(", ", message.Topics)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Kafka] Failed to start consuming");
            await _consumerInstance.ConsumingError(ex);
        }
    }

    private async Task HandleStopConsuming(StopConsumingMessage message)
    {
        try
        {
            _cancellationTokenSource?.Cancel();
            
            if (_consumingTask != null)
            {
                await _consumingTask;
            }
            
            lock (_consumerLock)
            {
                _consumer?.Unsubscribe();
            }
            
            await _consumerInstance.ConsumingStopped();
            
            _logger.LogInformation("[Kafka] Stopped consuming messages");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Kafka] Error stopping consumer");
        }
    }

    private async Task ConsumeLoop(CancellationToken cancellationToken)
    {
        var batch = new List<ConsumeResult<string, string>>();
        var batchStartTime = DateTime.UtcNow;
        
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Poll for messages with short timeout to allow batch processing
                    var consumeResult = _consumer.Consume(1000);
                    
                    if (consumeResult?.Message != null)
                    {
                        batch.Add(consumeResult);
                        
                        // Check if we should process the batch
                        var shouldProcess = batch.Count >= _processingConfig.MaxElements ||
                                          (DateTime.UtcNow - batchStartTime).TotalMilliseconds >= _processingConfig.TimeoutInMilliseconds;
                        
                        if (shouldProcess)
                        {
                            await ProcessBatch(batch, cancellationToken);
                            batch.Clear();
                            batchStartTime = DateTime.UtcNow;
                        }
                    }
                    else if (batch.Any())
                    {
                        // No new messages but we have a partial batch that's timed out
                        if ((DateTime.UtcNow - batchStartTime).TotalMilliseconds >= _processingConfig.TimeoutInMilliseconds)
                        {
                            await ProcessBatch(batch, cancellationToken);
                            batch.Clear();
                            batchStartTime = DateTime.UtcNow;
                        }
                    }
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, $"[Kafka] Consume error: {ex.Error.Reason}");
                    await _consumerInstance.ConsumingError(ex);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Kafka] Unexpected error in consume loop");
                    await _consumerInstance.ConsumingError(ex);
                }
            }
            
            // Process any remaining messages in batch
            if (batch.Any())
            {
                await ProcessBatch(batch, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Kafka] Fatal error in consume loop");
            await _consumerInstance.ConsumingError(ex);
        }
    }

    private async Task ProcessBatch(List<ConsumeResult<string, string>> consumeResults, CancellationToken cancellationToken)
    {
        if (!consumeResults.Any()) return;

        try
        {
            // Create batch context
            var kafkaBatch = new KafkaBatch
            {
                BatchStartTime = DateTime.UtcNow,
                TopicGroup = _metadata.TopicGroup,
                Messages = consumeResults.Select(MapToKafkaMessage).ToList(),
                PartitionOffsets = consumeResults
                    .GroupBy(r => r.TopicPartition)
                    .ToDictionary(g => g.Key, g => g.Max(r => r.Offset) + 1)
            };

            _context.CurrentBatch = kafkaBatch;

            // Deserialize messages to the expected type
            var messages = new List<object>();
            var targetType = GetMessageType();

            foreach (var result in consumeResults)
            {
                try
                {
                    var deserializedMessage = JsonConvert.DeserializeObject(result.Message.Value, targetType);
                    messages.Add(deserializedMessage);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[Kafka] Failed to deserialize message from {result.TopicPartition}:{result.Offset}");
                    // Could implement dead letter queue logic here
                }
            }

            // Invoke the handler method
            if (messages.Any())
            {
                var methodParams = new object[] { messages };
                await (Task)_metadata.HandlerMethod.Invoke(_consumerInstance, methodParams);
                
                kafkaBatch.BatchEndTime = DateTime.UtcNow;
                
                _logger.LogInformation(
                    $"[Kafka] ✅ Processed batch of {messages.Count} messages " +
                    $"(took {(kafkaBatch.BatchEndTime - kafkaBatch.BatchStartTime).TotalMilliseconds:F0}ms)");

                // Commit offsets after successful processing
                if (_processingConfig.CommitStrategy == OffsetCommitStrategy.AfterSuccessfulProcessing)
                {
                    var offsetsToCommit = kafkaBatch.PartitionOffsets
                        .Select(po => new TopicPartitionOffset(po.Key, po.Value))
                        .ToList();
                    
                    lock (_consumerLock)
                    {
                        _consumer.Commit(offsetsToCommit);
                    }
                    
                    _logger.LogDebug($"[Kafka] Committed offsets for {offsetsToCommit.Count} partitions");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[Kafka] ❌ Failed to process batch of {consumeResults.Count} messages");
            
            // Don't commit offsets on failure - messages will be reprocessed
            await _consumerInstance.ConsumingError(ex);
            
            // Could implement retry logic or dead letter queue here
        }
    }

    private KafkaMessage MapToKafkaMessage(ConsumeResult<string, string> result)
    {
        return new KafkaMessage
        {
            Topic = result.Topic,
            Partition = result.Partition.Value,
            Offset = result.Offset.Value,
            Key = result.Message.Key,
            Value = result.Message.Value,
            Timestamp = result.Message.Timestamp.UtcDateTime,
            Headers = result.Message.Headers
        };
    }

    private Type GetMessageType()
    {
        // Get the generic type parameter from the handler method
        var parameters = _metadata.HandlerMethod.GetParameters();
        if (parameters.Length > 0)
        {
            var listType = parameters[0].ParameterType;
            if (listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof(List<>))
            {
                return listType.GetGenericArguments()[0];
            }
        }
        
        // Fallback to object if we can't determine the type
        return typeof(object);
    }

    protected override void PostStop()
    {
        try
        {
            _cancellationTokenSource?.Cancel();
            _consumingTask?.Wait(5000);
            
            lock (_consumerLock)
            {
                _consumer?.Close();
                _consumer?.Dispose();
            }
            
            _logger.LogInformation("[Kafka] Consumer actor stopped and disposed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Kafka] Error disposing consumer");
        }
        
        base.PostStop();
    }
}
