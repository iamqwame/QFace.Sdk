# QFace AI SDK Minimal API Demo

## Prerequisites
- .NET 9.0 SDK
- OpenAI API key (for LLM features)
- Optional: Anthropic API key, Google Gemini API key

## Project Structure
- Minimal API approach
- Built with .NET 9.0
- Swagger/OpenAPI integration
- AI SDK extensions

## Configuration
Update `appsettings.json` with your AI provider credentials:

```json
{
  "AI": {
    "DefaultLLMProvider": "OpenAI",
    "DefaultForecastMethod": "Regression",
    "EnableMLForecasting": false,
    "OpenAI": {
      "ApiKey": "your-openai-api-key",
      "DefaultModel": "gpt-4",
      "MaxTokens": 2000,
      "Temperature": 0.7
    },
    "Anthropic": {
      "ApiKey": "your-anthropic-api-key",
      "DefaultModel": "claude-3-5-sonnet-20241022"
    },
    "GoogleGemini": {
      "ApiKey": "your-google-api-key",
      "DefaultModel": "gemini-pro"
    }
  }
}
```

## Endpoints

### Forecasting Endpoints

#### Trend Analysis Forecast
`POST /api/forecast/trend`
- Simple linear trend calculation
- Requires historical data points
- Returns forecast with confidence intervals

#### Regression Analysis Forecast
`POST /api/forecast/regression`
- Linear regression using MathNet.Numerics
- Provides R-squared metrics
- Returns forecast with confidence intervals

#### ML Forecast
`POST /api/forecast/ml`
- ML.NET time series forecasting
- Requires EnableMLForecasting to be true
- More accurate for complex patterns

#### Manual Forecast
`POST /api/forecast/manual`
- Uses assumptions from request
- No historical data required
- Perfect for scenario planning

#### Forecast with Method Selection
`POST /api/forecast?method={Trend|Regression|ML|Manual}`
- Flexible method selection via query parameter
- Uses ForecastMethod enum

### LLM Endpoints

#### Generate Completion
`POST /api/llm/completion`
- Single prompt completion
- Supports all configured providers
- Returns generated content with token usage

#### Generate Chat Completion
`POST /api/llm/chat`
- Multi-turn conversation support
- System, user, and assistant messages
- Context-aware responses

### Skills Analysis Endpoints

#### Analyze Skills Gap
`POST /api/skills/analyze`
- Rule-based or LLM-based analysis
- Compares required vs current skills
- Returns gap analysis with severity levels

#### Generate Recommendations
`POST /api/skills/recommendations`
- Generates actionable recommendations
- Prioritizes by severity
- Suggests training, hiring, or external options

### API Information
`GET /api/info`
- Returns complete API overview
- Lists all available endpoints
- Provides usage examples

## Example Requests

### Trend Forecast
```bash
curl -X POST http://localhost:5000/api/forecast/trend \
  -H "Content-Type: application/json" \
  -d '{
    "historicalData": [
      {"date": "2024-01-01T00:00:00Z", "value": 100},
      {"date": "2024-02-01T00:00:00Z", "value": 120},
      {"date": "2024-03-01T00:00:00Z", "value": 140}
    ],
    "targetDate": "2024-06-01T00:00:00Z",
    "confidenceLevel": 80
  }'
```

### LLM Completion
```bash
curl -X POST http://localhost:5000/api/llm/completion \
  -H "Content-Type: application/json" \
  -d '{
    "prompt": "Explain AI in simple terms",
    "provider": "OpenAI",
    "model": "gpt-4"
  }'
```

### Skills Gap Analysis
```bash
curl -X POST http://localhost:5000/api/skills/analyze \
  -H "Content-Type: application/json" \
  -d '{
    "requiredSkills": ["C#", "Docker", "Kubernetes"],
    "currentSkills": ["C#", "SQL"],
    "useLLMForAnalysis": false
  }'
```

## Key Features
- **Multiple Forecasting Methods**: Trend, Regression, ML, Manual
- **LLM Provider Abstraction**: Unified interface for OpenAI, Anthropic, Google Gemini
- **Skills Analysis**: Rule-based and LLM-powered gap analysis
- **Swagger UI**: Interactive API documentation
- **Flexible Configuration**: Easy provider switching
- **Comprehensive Logging**: Full diagnostic support

## Running the Project
1. Configure `appsettings.json` with your API keys
2. Run `dotnet restore`
3. Run `dotnet run`
4. Open Swagger UI at `/swagger`

## Forecast Methods Comparison

| Method | Best For | Data Requirements | Accuracy |
|--------|----------|-------------------|----------|
| **Trend** | Simple linear trends | 2+ data points | Basic |
| **Regression** | Linear relationships | 5+ data points | Good |
| **ML** | Complex patterns | 10+ data points | Best |
| **Manual** | Scenario planning | None | User-defined |

## Notes
- OpenAI provider is fully functional with Microsoft.Extensions.AI
- Anthropic and Google Gemini providers are stubs (packages not yet available)
- ML forecasting requires EnableMLForecasting=true in configuration
- All endpoints support async operations with cancellation tokens

