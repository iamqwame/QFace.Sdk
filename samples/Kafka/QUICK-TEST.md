# Test Instructions for Kafka SDK

## Quick Compilation Test

Before running the full demo, let's test if the Kafka SDK compiles:

```bash
# 1. Clean and build the Kafka SDK project
cd /Users/iamqwame/MaProjects/QFace.Sdk/src/QFace.Sdk.Kafka
dotnet clean
dotnet build

# 2. If successful, build the demo projects
cd /Users/iamqwame/MaProjects/QFace.Sdk/samples/Kafka/Demo.Kafka.Api
dotnet build

cd /Users/iamqwame/MaProjects/QFace.Sdk/samples/Kafka/Demo.Kafka.Consumer  
dotnet build
```

## If Build Succeeds

```bash
# 1. Start Consumer (will show any runtime issues)
cd /Users/iamqwame/MaProjects/QFace.Sdk/samples/Kafka/Demo.Kafka.Consumer
dotnet run

# 2. In another terminal, start Producer API
cd /Users/iamqwame/MaProjects/QFace.Sdk/samples/Kafka/Demo.Kafka.Api
dotnet run

# 3. Test with simple curl (no Kafka needed for actor initialization test)
curl -X POST http://localhost:5109/publish \
  -H "Content-Type: application/json" \
  -d '{"eventType":"test","data":{"message":"hello"}}'
```

## Expected Behavior

✅ **If actor initialization fixed:** Producer service should create actor successfully  
❌ **If still failing:** Will get the same ActorNotFoundException

Let me know which step fails and what error you see!
