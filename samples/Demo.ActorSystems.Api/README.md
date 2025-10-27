# QFace Actor Systems Demo API

This demo API showcases the QFace.Sdk.ActorSystems functionality with a simple actor for testing message passing.

## Features

### Actors
- **SimpleTestActor**: Receives and processes string messages

### API Endpoints

#### Test API (`/api/test`)
- `POST /api/test/send-message` - Send a message to the actor system
- `GET /api/test/status` - Get actor system status

#### System Endpoints
- `GET /health` - Health check
- `GET /actor-system/test` - Direct actor system test

## Configuration

The API uses the following configuration sections:

### Logging (`Logs`)
```json
{
  "Logs": {
    "Using": "Graylog",
    "Url": "174.138.46.233",
    "Port": 12201,
    "Facility": "QFace.ActorSystemsDemo",
    "MinimumLevel": "Information"
  }
}
```

### Actor System (`ActorSystem`)
```json
{
  "ActorSystem": {
    "SystemName": "ActorSystemsDemo",
    "Config": {
      "LogLevel": "Info",
      "LogConfig": "akka.loglevel=INFO"
    }
  }
}
```

## Running the Demo

1. **Build the project**:
   ```bash
   dotnet build
   ```

2. **Run the API**:
   ```bash
   dotnet run
   ```

3. **Access Swagger UI**: `https://localhost:5001/swagger` (or the configured port)

## Testing the Actor System

### 1. Health Check
```bash
curl http://localhost:5088/health
```

### 2. Test Actor System
```bash
curl http://localhost:5088/actor-system/test
```

### 3. Send Message via API
```bash
curl -X POST http://localhost:5088/api/test/send-message \
  -H "Content-Type: application/json" \
  -d '{"message": "Hello from API!"}'
```

### 4. Get Actor System Status
```bash
curl http://localhost:5088/api/test/status
```

## Architecture

The demo demonstrates:
- **Actor-based architecture** using Akka.NET
- **Message passing** between actors
- **HTTP API integration** with actor system
- **Structured logging** with QFace logging
- **Error handling** and response patterns
- **Actor lifecycle management**

## Logging

The application logs actor operations, HTTP requests, and system events. In development mode, debug-level logging is enabled to show detailed actor system activity.
