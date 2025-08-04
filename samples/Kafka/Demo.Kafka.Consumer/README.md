# Demo.Kafka.Consumer - Kafka Consumer Demo

Demonstrates QFace Kafka SDK consumer capabilities with your exact pattern.

## 🚀 **Quick Start**
```bash
dotnet run
```

## 🎯 **Demonstrates Your Pattern**

```csharp
public class EventAnalyticsConsumer : KafkaConsumerBase
{
    [ConsumeTopic("Analytics")]
    public async Task HandleAnalyticsEvents(List<EventSourceModel> events)
    {
        // Your bulk processing logic
    }
}
```

## 📊 **Topic Groups**
- **Analytics**: `demo.events`, `analytics.events`  
- **UserEvents**: `user.created`, `user.updated`, `user.deleted`
- **SystemMonitoring**: `system.metric`, `system.health`, `system.alerts`
- **Critical Alerts**: Direct topics `alerts.critical`, `alerts.emergency`

## 🔧 **Configuration**
```json
{
  "KafkaConsumerConfig": {
    "GroupId": "demo-analytics-group",
    "TopicGroups": {
      "Analytics": ["demo.events", "analytics.events"]
    },
    "MaxBatchSize": 100,
    "BatchTimeoutMs": 3000
  }
}
```

## 📝 **What You'll See**
- Batch processing logs with event counts
- Different handlers for different topic groups
- Partition assignment/revocation logs
- Event type routing within handlers
- Performance metrics (batch sizes, processing times)

Perfect for testing your consumer pattern with topic groups!
