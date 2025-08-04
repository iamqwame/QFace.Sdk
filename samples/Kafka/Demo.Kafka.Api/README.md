# Demo.Kafka.Api - Kafka Producer Demo

Demonstrates QFace Kafka SDK producer capabilities with minimal API endpoints.

## 🚀 **Quick Start**
```bash
dotnet run
```
Open: https://localhost:7109/swagger

## 📡 **API Endpoints**

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/publish` | Publish single event to demo.events |
| `POST` | `/publish-batch` | Publish multiple events at once |
| `POST` | `/publish-to-topic` | Publish to any topic with custom key |

## 🧪 **Test Commands**

```bash
# Simple event
curl -X POST https://localhost:7109/publish \
  -H "Content-Type: application/json" \
  -d '{"eventType":"user.created","data":{"userId":"123"}}'

# Batch events  
curl -X POST https://localhost:7109/publish-batch \
  -H "Content-Type: application/json" \
  -d '[{"eventType":"user.created","data":{"userId":"1"}},{"eventType":"user.updated","data":{"userId":"2"}}]'
```

## ⚙️ **Configuration**
```json
{
  "KafkaProducerConfig": {
    "BootstrapServers": "localhost:9092"
  }
}
```

Perfect for testing Kafka producer functionality!
