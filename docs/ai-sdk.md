# QFace.Sdk.AI

A generic AI/ML SDK library for .NET applications. Supports multiple LLM providers (OpenAI, Anthropic, Google Gemini), forecasting algorithms (Trend, Regression, ML, Manual), and skills analysis. Designed to be extensible and provider-agnostic.

## Features

- **Multiple LLM Providers**: Unified interface for OpenAI, Anthropic, and Google Gemini
- **Forecasting Algorithms**: Trend analysis, Linear regression, ML.NET time series, and Manual forecasting
- **Skills Analysis**: Rule-based and LLM-powered skills gap analysis
- **Provider-Agnostic**: Switch between LLM providers without changing code
- **Extensible**: Easy to add new providers and algorithms
- **Async/Await**: Fully asynchronous API
- **Comprehensive Logging**: Built-in logging for diagnostics

## Installation

```shell
dotnet add package QFace.Sdk.AI
```

## Configuration

Add the following configuration to your `appsettings.json`:

```json
{
  "AI": {
    "DefaultLLMProvider": "GoogleGemini",
    "DefaultForecastMethod": "Regression",
    "EnableMLForecasting": false,
    "MLModelPath": "./ml-models",
    "OpenAI": {
      "ApiKey": "your-openai-api-key",
      "BaseUrl": "https://api.openai.com/v1",
      "DefaultModel": "gpt-4",
      "MaxTokens": 2000,
      "Temperature": 0.7
    },
    "Anthropic": {
      "ApiKey": "your-anthropic-api-key",
      "BaseUrl": "https://api.anthropic.com/v1",
      "DefaultModel": "claude-3-5-sonnet-20241022",
      "MaxTokens": 2000
    },
    "GoogleGemini": {
      "ApiKey": "your-google-api-key",
      "BaseUrl": "https://generativelanguage.googleapis.com/v1",
      "DefaultModel": "gemini-2.5-flash",
      "MaxTokens": 2000
    }
  }
}
```

### Configuration Options

- **DefaultLLMProvider**: The default LLM provider to use (`OpenAI`, `Anthropic`, or `GoogleGemini`)
- **DefaultForecastMethod**: Default forecasting method (`Trend`, `Regression`, `ML`, or `Manual`)
- **EnableMLForecasting**: Enable ML.NET time series forecasting (requires model training)
- **MLModelPath**: Path where ML models are stored
- **Provider-specific settings**: Each provider has its own configuration section

## Usage

### Registration

```csharp
// In Program.cs or Startup.cs
using QFace.Sdk.AI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Register AI services
builder.Services.AddAIServices(builder.Configuration);
```

### LLM Services

#### Basic Completion

```csharp
using QFace.Sdk.AI.Services;
using QFace.Sdk.AI.Models;

public class MyService
{
    private readonly ILLMService _llmService;

    public MyService(ILLMService llmService)
    {
        _llmService = llmService;
    }

    public async Task<string> GetAIResponse(string prompt)
    {
        var request = new LLMRequest
        {
            Prompt = prompt,
            Provider = "GoogleGemini", // Optional: uses default if not specified
            MaxTokens = 500,
            Temperature = 0.7
        };

        var response = await _llmService.GenerateCompletionAsync(request);
        return response.Content;
    }
}
```

#### Chat Completion

```csharp
public async Task<string> GetChatResponse()
{
    var request = new LLMRequest
    {
        Messages = new List<LLMMessage>
        {
            new LLMMessage
            {
                Role = "system",
                Content = "You are a helpful AI assistant."
            },
            new LLMMessage
            {
                Role = "user",
                Content = "What is the capital of France?"
            }
        },
        Provider = "GoogleGemini",
        MaxTokens = 200
    };

    var response = await _llmService.GenerateChatCompletionAsync(request);
    return response.Content;
}
```

#### Provider Selection

You can specify a provider per request or use the default:

```csharp
// Use default provider (from configuration)
var request1 = new LLMRequest { Prompt = "Hello" };

// Override provider for this request
var request2 = new LLMRequest 
{ 
    Prompt = "Hello",
    Provider = "OpenAI" // Override default
};
```

### Forecasting Services

#### Trend Analysis

Simple linear trend calculation - best for basic projections:

```csharp
using QFace.Sdk.AI.Services;
using QFace.Sdk.AI.Models;

public class ForecastService
{
    private readonly IForecastingService _forecastingService;

    public ForecastService(IForecastingService forecastingService)
    {
        _forecastingService = forecastingService;
    }

    public async Task<ForecastResult> ForecastTrend()
    {
        var request = new ForecastRequest
        {
            HistoricalData = new List<HistoricalDataPoint>
            {
                new HistoricalDataPoint { Date = new DateTime(2024, 1, 1), Value = 100 },
                new HistoricalDataPoint { Date = new DateTime(2024, 2, 1), Value = 120 },
                new HistoricalDataPoint { Date = new DateTime(2024, 3, 1), Value = 140 },
                new HistoricalDataPoint { Date = new DateTime(2024, 4, 1), Value = 160 },
                new HistoricalDataPoint { Date = new DateTime(2024, 5, 1), Value = 180 }
            },
            TargetDate = new DateTime(2024, 6, 1),
            Method = ForecastMethod.Trend,
            ConfidenceLevel = 80
        };

        return await _forecastingService.CalculateForecastAsync(request);
    }
}
```

#### Regression Analysis

Linear regression using MathNet.Numerics - provides R-squared metrics and confidence intervals:

```csharp
public async Task<ForecastResult> ForecastRegression()
{
    var request = new ForecastRequest
    {
        HistoricalData = new List<HistoricalDataPoint>
        {
            new HistoricalDataPoint { Date = new DateTime(2024, 1, 1), Value = 100 },
            new HistoricalDataPoint { Date = new DateTime(2024, 2, 1), Value = 120 },
            new HistoricalDataPoint { Date = new DateTime(2024, 3, 1), Value = 140 },
            new HistoricalDataPoint { Date = new DateTime(2024, 4, 1), Value = 160 },
            new HistoricalDataPoint { Date = new DateTime(2024, 5, 1), Value = 180 }
        },
        TargetDate = new DateTime(2024, 6, 1),
        Method = ForecastMethod.Regression,
        ConfidenceLevel = 95
    };

    var result = await _forecastingService.CalculateForecastAsync(request);
    
    // Access regression metrics
    var rSquared = result.Metadata?["RSquared"];
    var slope = result.Metadata?["Slope"];
    
    return result;
}
```

#### ML.NET Forecasting

Machine learning-based forecasting - most accurate for complex patterns:

```csharp
public async Task<ForecastResult> ForecastML()
{
    var request = new ForecastRequest
    {
        HistoricalData = GetHistoricalData(), // 10+ data points recommended
        TargetDate = new DateTime(2024, 12, 1),
        Method = ForecastMethod.ML,
        ConfidenceLevel = 90
    };

    return await _forecastingService.CalculateForecastAsync(request);
}
```

**Note**: ML forecasting requires `EnableMLForecasting: true` in configuration.

#### Manual Forecasting

Use assumptions for scenario planning:

```csharp
public async Task<ForecastResult> ForecastManual()
{
    var request = new ForecastRequest
    {
        TargetDate = new DateTime(2024, 12, 1),
        Method = ForecastMethod.Manual,
        Assumptions = new Dictionary<string, object>
        {
            { "value", 250 },
            { "lowerBound", 200 },
            { "upperBound", 300 }
        },
        ConfidenceLevel = 100
    };

    return await _forecastingService.CalculateForecastAsync(request);
}
```

#### Method Selection

You can specify the method per request or use the default:

```csharp
// Use method from request
var request1 = new ForecastRequest 
{ 
    Method = ForecastMethod.Regression,
    // ... other properties
};

// Override method via parameter
var result = await _forecastingService.CalculateForecastAsync(
    request1, 
    method: ForecastMethod.Trend
);

// Use default method (from configuration)
var request2 = new ForecastRequest 
{ 
    // Method not specified - uses DefaultForecastMethod
    // ... other properties
};
```

### Skills Analysis Services

#### Rule-Based Analysis

Simple comparison of required vs current skills:

```csharp
using QFace.Sdk.AI.Services;
using QFace.Sdk.AI.Models;

public class SkillsService
{
    private readonly ISkillsAnalysisService _skillsAnalysisService;

    public SkillsService(ISkillsAnalysisService skillsAnalysisService)
    {
        _skillsAnalysisService = skillsAnalysisService;
    }

    public async Task<SkillsGapResult> AnalyzeSkillsGap()
    {
        var request = new SkillsGapRequest
        {
            RequiredSkills = new List<string>
            {
                "C#",
                "ASP.NET Core",
                "Entity Framework",
                "Docker",
                "Kubernetes",
                "Microservices Architecture"
            },
            CurrentSkills = new List<string>
            {
                "C#",
                "ASP.NET Core",
                "Entity Framework",
                "SQL Server"
            },
            UseLLMForAnalysis = false // Use rule-based analysis
        };

        return await _skillsAnalysisService.AnalyzeSkillsGapAsync(request);
    }
}
```

#### LLM-Powered Analysis

Use AI for intelligent skills gap analysis:

```csharp
public async Task<SkillsGapResult> AnalyzeSkillsGapWithAI()
{
    var request = new SkillsGapRequest
    {
        RequiredSkills = new List<string>
        {
            "C#",
            "ASP.NET Core",
            "Docker",
            "Kubernetes",
            "Cloud Computing",
            "CI/CD"
        },
        CurrentSkills = new List<string>
        {
            "C#",
            "ASP.NET Core",
            "SQL Server",
            "Git"
        },
        UseLLMForAnalysis = true, // Use LLM for analysis
        LLMProvider = "GoogleGemini" // Optional: override default provider
    };

    var result = await _skillsAnalysisService.AnalyzeSkillsGapAsync(request);
    
    // Access gap analysis results
    var gapSkills = result.GapSkills; // ["Docker", "Kubernetes", ...]
    var matchedSkills = result.MatchedSkills; // ["C#", "ASP.NET Core", ...]
    var severity = result.GapSeverity; // Dictionary of skill -> severity
    
    return result;
}
```

#### Generate Recommendations

Get actionable recommendations based on skills gap:

```csharp
public async Task<List<SkillsRecommendation>> GetRecommendations(SkillsGapResult gapAnalysis)
{
    var recommendations = await _skillsAnalysisService
        .GenerateRecommendationsAsync(gapAnalysis);
    
    foreach (var recommendation in recommendations)
    {
        Console.WriteLine($"{recommendation.Skill}: {recommendation.Description}");
        Console.WriteLine($"Priority: {recommendation.Priority}");
        Console.WriteLine($"Type: {recommendation.RecommendationType}");
    }
    
    return recommendations;
}
```

## Forecast Methods Comparison

| Method | Best For | Data Requirements | Accuracy | Speed |
|--------|----------|-------------------|---------|-------|
| **Trend** | Simple linear trends | 2+ data points | Basic | Fastest |
| **Regression** | Linear relationships | 5+ data points | Good | Fast |
| **ML** | Complex patterns | 10+ data points | Best | Slower |
| **Manual** | Scenario planning | None | User-defined | Instant |

## LLM Providers

### Supported Providers

1. **OpenAI** - GPT-4, GPT-3.5, and other OpenAI models
2. **Anthropic** - Claude models (Claude 3.5 Sonnet, etc.)
3. **Google Gemini** - Gemini 2.5 Flash, Gemini 2.5 Pro, etc.

### Provider Status

- **Google Gemini**: ✅ Fully implemented and tested
- **OpenAI**: ⚠️ Stub implementation (requires OpenAI SDK package)
- **Anthropic**: ⚠️ Stub implementation (requires Anthropic SDK package)

### Switching Providers

You can switch providers at runtime:

```csharp
// Use different providers for different requests
var openAIRequest = new LLMRequest 
{ 
    Prompt = "Explain AI",
    Provider = "OpenAI"
};

var geminiRequest = new LLMRequest 
{ 
    Prompt = "Explain AI",
    Provider = "GoogleGemini"
};
```

## Advanced Usage

### Multiple Forecasts

Calculate multiple forecasts in parallel:

```csharp
var requests = new List<ForecastRequest>
{
    new ForecastRequest { /* ... */ },
    new ForecastRequest { /* ... */ },
    new ForecastRequest { /* ... */ }
};

var results = await _forecastingService
    .CalculateMultipleForecastsAsync(requests);
```

### Custom Model Selection

Override the default model per request:

```csharp
var request = new LLMRequest
{
    Prompt = "Complex question",
    Provider = "GoogleGemini",
    Model = "gemini-2.5-pro", // Override default model
    MaxTokens = 4000,
    Temperature = 0.9
};
```

### Error Handling

```csharp
try
{
    var result = await _forecastingService.CalculateForecastAsync(request);
    
    if (result.Warnings?.Any() == true)
    {
        foreach (var warning in result.Warnings)
        {
            _logger.LogWarning("Forecast warning: {Warning}", warning);
        }
    }
}
catch (ArgumentException ex)
{
    _logger.LogError(ex, "Invalid forecast request");
}
catch (Exception ex)
{
    _logger.LogError(ex, "Forecast calculation failed");
}
```

## Best Practices

1. **Forecasting**:
   - Use Trend for simple projections with limited data
   - Use Regression for linear relationships (recommended default)
   - Use ML for complex patterns with sufficient historical data
   - Use Manual for scenario planning

2. **LLM Usage**:
   - Use chat completion for multi-turn conversations
   - Set appropriate `MaxTokens` to control response length
   - Adjust `Temperature` for creativity (0.0 = deterministic, 2.0 = creative)

3. **Skills Analysis**:
   - Use rule-based analysis for simple comparisons
   - Use LLM-powered analysis for intelligent gap detection
   - Generate recommendations after gap analysis

4. **Configuration**:
   - Store API keys in secure configuration (User Secrets, Azure Key Vault, etc.)
   - Use environment-specific configurations
   - Enable ML forecasting only when needed (has performance overhead)

## Troubleshooting

### LLM Provider Not Working

1. Check API key is correctly configured
2. Verify provider is available: `await provider.IsAvailableAsync()`
3. Check network connectivity
4. Review logs for detailed error messages

### Forecast Returns Warnings

- **Limited historical data**: Add more data points for better accuracy
- **Insufficient data for ML**: Use Trend or Regression instead
- **Low confidence**: Review data quality and patterns

### ML Forecasting Not Available

- Ensure `EnableMLForecasting: true` in configuration
- Check that `MLModelPath` is writable
- Verify Microsoft.ML packages are installed

## Examples

See the `/samples/Demo.AI.Api` directory for complete working examples including:

- Minimal API endpoints for all services
- Configuration examples
- Error handling patterns
- Integration with ASP.NET Core

## API Reference

### Services

- `IForecastingService` - Forecasting calculations
- `ILLMService` - LLM completions and chat
- `ISkillsAnalysisService` - Skills gap analysis

### Models

- `ForecastRequest` / `ForecastResult` - Forecasting models
- `LLMRequest` / `LLMResponse` - LLM models
- `SkillsGapRequest` / `SkillsGapResult` - Skills analysis models
- `AIOptions` - Configuration options

### Enums

- `ForecastMethod` - Available forecasting methods (Trend, Regression, ML, Manual)

## License

MIT License - See LICENSE file for details.

