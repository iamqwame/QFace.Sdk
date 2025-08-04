# QFace Kafka SDK - Demo Applications

This folder contains demo applications showcasing the QFace Kafka SDK with your **exact usage pattern**.

## 📁 **Project Structure**

```
samples/Kafka/
├── Demo.Kafka.Api/          # 🚀 Producer API (publishes events)
│   ├── Program.cs           # Web API with Kafka producer endpoints
│   ├── appsettings.json     # Kafka producer configuration
│   └── Demo.Kafka.Api.http  # HTTP test file
└── Demo.Kafka.Consumer/     # 📥 Consumer App (processes events)
    ├── Program.cs           # Console app with Kafka consumer
    ├── EventAnalyticsConsumer.cs  # Your exact consumer pattern
    └── appsettings.json     # Kafka consumer with topic groups
```

## 🎯 **Demonstrates Your Exact Pattern**

### **Consumer Pattern (Your Style)**
```csharp
public class EventAnalyticsConsumer : KafkaConsumerBase
{
    public EventAnalyticsConsumer(ILogger<EventAnalyticsConsumer> logger, ITopLevelActors topLevelActors) { }

    [ConsumeTopic("Analytics")]           // ← Your exact pattern
    public async Task HandleAnalyticsEvents(List<EventSourceModel> events)
    {
        // Your bulk processing logic
    }
}
```

### **Topic Groups Configuration (Your Approach)**
```json
{
  "KafkaConsumerConfig": {
    "TopicGroups": {
      "Analytics": ["demo.events", "analytics.events"],
      "UserEvents": ["user.created", "user.updated", "user.deleted"],
      "SystemMonitoring": ["system.metric", "system.health"]
    }
  }
}
```

## 🚀 **Quick Start**

### **1. Start Kafka (Local)**
```bash
# Using Docker Compose
docker run -d \
  --name kafka \
  -p 9092:9092 \
  -e KAFKA_ZOOKEEPER_CONNECT=zookeeper:2181 \
  -e KAFKA_ADVERTISED_LISTENERS=PLAINTEXT://localhost:9092 \
  -e KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR=1 \
  confluentinc/cp-kafka:latest

# Or use your existing Kafka setup
```

### **2. Run Consumer (Terminal 1)**
```bash
cd /Users/iamqwame/MaProjects/QFace.Sdk/samples/Kafka/Demo.Kafka.Consumer
dotnet run
```

### **3. Run Producer API (Terminal 2)**
```bash
cd /Users/iamqwame/MaProjects/QFace.Sdk/samples/Kafka/Demo.Kafka.Api
dotnet run
```

### **4. Open Swagger UI**
Navigate to: https://localhost:7109/swagger

### **5. Test Publishing**
Use the HTTP file or Swagger to publish test events

## 📊 **What Gets Demonstrated**

### **✅ Producer Capabilities**
- **Simple Event Publishing**: `POST /publish`
- **Batch Publishing**: `POST /publish-batch`
- **Custom Topic Publishing**: `POST /publish-to-topic`
- **Key-based Partitioning**: Events routed by event type
- **Actor-based Production**: Uses your actor system pattern

### **✅ Consumer Capabilities** 
- **Topic Group Processing**: Multiple topic groups per consumer
- **Bulk Message Processing**: Your 100-message, 3-second batch pattern
- **Event Type Routing**: Different handlers for different event types
- **Partition Awareness**: Logs partition assignments/revocations
- **Error Handling**: Comprehensive error logging and recovery
- **Lifecycle Management**: Startup/shutdown hooks

### **✅ Event Flow Examples**

1. **Analytics Events**:
   ```
   API → demo.events → Analytics Topic Group → HandleAnalyticsEvents()
   ```

2. **User Events**:
   ```
   API → user.created → UserEvents Topic Group → HandleUserEvents()
   ```

3. **Critical Alerts**:
   ```
   API → alerts.critical → Direct Topic → HandleCriticalAlerts()
   ```

## 🧪 **Test Scenarios**

### **Scenario 1: User Registration Flow**
```bash
# Publish user created event
POST /publish
{
  "eventType": "user.created",
  "data": {
    "userId": "user123",
    "username": "johnsmith",
    "email": "john@example.com"
  }
}

# Check consumer logs for:
# ✅ Analytics processing (demo.events topic)
# ✅ User event processing (UserEvents topic group)
```

### **Scenario 2: System Monitoring**
```bash
# Publish system metric
POST /publish
{
  "eventType": "system.metric", 
  "data": {
    "metricName": "cpu.usage",
    "value": 75.5,
    "unit": "percent"
  }
}

# Check consumer logs for SystemMonitoring processing
```

### **Scenario 3: Batch Processing**
```bash
# Publish multiple events at once
POST /publish-batch
[
  { "eventType": "user.created", "data": {...} },
  { "eventType": "user.updated", "data": {...} },
  { "eventType": "system.metric", "data": {...} }
]

# Watch consumer batch processing in action
```

### **Scenario 4: Critical Alerts**
```bash
# Publish to critical alerts topic
POST /publish-to-topic
{
  "topic": "alerts.critical",
  "key": "system.error",
  "data": {
    "severity": "critical",
    "message": "Database connection lost"
  }
}

# Check consumer handles critical alerts immediately
```

## 📈 **Performance Testing**

### **Load Testing**
```bash
# Publish 100 events rapidly
for i in {1..100}; do
  curl -X POST https://localhost:7109/publish \
    -H "Content-Type: application/json" \
    -d '{"eventType":"load.test","data":{"iteration":'$i'}}'
done

# Watch batch processing efficiency in consumer logs
```

### **Partition Testing**
```bash
# Publish events with same key (same partition)
POST /publish-to-topic
{
  "topic": "demo.events",
  "key": "same-key",  # ← Same partition
  "data": {"message": "Event 1"}
}

# Check partition assignment in consumer logs
```

## 🔧 **Configuration Options**

### **Producer Configuration**
```json
{
  "KafkaProducerConfig": {
    "BootstrapServers": "localhost:9092",
    "ExtraProperties": {
      "acks": "all",                    // Wait for all replicas
      "retries": "5",                   // Retry failed sends  
      "enable.idempotence": "true"      // Prevent duplicates
    }
  }
}
```

### **Consumer Configuration**
```json
{
  "KafkaConsumerConfig": {
    "BootstrapServers": "localhost:9092",
    "GroupId": "demo-analytics-group",
    "TopicGroups": {
      "Analytics": ["demo.events", "analytics.events"],
      "UserEvents": ["user.created", "user.updated", "user.deleted"]
    },
    "MaxBatchSize": 100,
    "BatchTimeoutMs": 3000,
    "ExtraProperties": {
      "auto.offset.reset": "latest"
    }
  },
  "MessageGroupConsumerLogicConfig": {
    "TimeoutInMilliseconds": 3000,
    "MaxElements": 100
  }
}
```

## 🐛 **Troubleshooting**

### **Common Issues**

1. **Kafka Not Running**
   ```bash
   # Check if Kafka is running
   netstat -an | grep 9092
   
   # Start Kafka if needed
   # (instructions depend on your Kafka setup)
   ```

2. **Consumer Not Receiving Messages**
   - Check topic group configuration
   - Verify consumer group ID is unique
   - Check Kafka topic exists: `kafka-topics.sh --list --bootstrap-server localhost:9092`

3. **Producer Connection Issues**
   - Verify `BootstrapServers` in configuration
   - Check API starts without errors
   - Test health endpoint in Swagger

### **Debug Logging**
Enable detailed logging in `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "QFace.Sdk.Kafka": "Debug",
      "Confluent.Kafka": "Information"
    }
  }
}
```

## 🎉 **Success Indicators**

✅ **Producer API starts successfully**  
✅ **Consumer app starts without errors**  
✅ **Swagger UI accessible at https://localhost:7109/swagger**  
✅ **Messages published via API appear in consumer logs**  
✅ **Batch processing works (multiple events processed together)**  
✅ **Topic groups route to correct handlers**  
✅ **Partition assignments logged correctly**  

## 🔄 **Next Steps**

1. **Test with your real Kafka cluster** - Update BootstrapServers
2. **Add your event types** - Extend EventSourceModel for your use cases
3. **Implement your business logic** - Replace simulation code with real processing
4. **Scale horizontally** - Run multiple consumer instances to see load balancing
5. **Monitor performance** - Check batch sizes and processing times

---

**Demo Applications** - Showcasing QFace Kafka SDK with your exact patterns! 🚀
