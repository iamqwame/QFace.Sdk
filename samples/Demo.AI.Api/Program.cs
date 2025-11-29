using Microsoft.AspNetCore.Mvc;
using QFace.Sdk.AI.Extensions;
using QFace.Sdk.AI.Models;
using QFace.Sdk.AI.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "AI SDK API", 
        Version = "v1",
        Description = "Demo API showcasing QFace.Sdk.AI capabilities: Forecasting, LLM completions, and Skills Analysis"
    });
});

// Configure AI Services
builder.Services.AddAIServices(builder.Configuration);

// Add logging
builder.Logging.AddConsole();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AI SDK API v1"));
}

app.UseHttpsRedirection();

// ============================================
// Forecasting Endpoints
// ============================================

// Forecast using Trend Analysis
app.MapPost("/api/forecast/trend", async (
    [FromBody] ForecastRequest request,
    IForecastingService forecastingService,
    ILogger<Program> logger) =>
{
    try
    {
        request.Method = ForecastMethod.Trend;
        var result = await forecastingService.CalculateForecastAsync(request);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Trend forecast failed");
        return Results.StatusCode(500);
    }
})
.WithName("ForecastTrend")
.WithOpenApi();

// Forecast using Regression Analysis
app.MapPost("/api/forecast/regression", async (
    [FromBody] ForecastRequest request,
    IForecastingService forecastingService,
    ILogger<Program> logger) =>
{
    try
    {
        request.Method = ForecastMethod.Regression;
        var result = await forecastingService.CalculateForecastAsync(request);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Regression forecast failed");
        return Results.StatusCode(500);
    }
})
.WithName("ForecastRegression")
.WithOpenApi();

// Forecast using ML
app.MapPost("/api/forecast/ml", async (
    [FromBody] ForecastRequest request,
    IForecastingService forecastingService,
    ILogger<Program> logger) =>
{
    try
    {
        request.Method = ForecastMethod.ML;
        var result = await forecastingService.CalculateForecastAsync(request);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "ML forecast failed");
        return Results.BadRequest(new { Message = ex.Message });
    }
})
.WithName("ForecastML")
.WithOpenApi();

// Forecast using Manual method
app.MapPost("/api/forecast/manual", async (
    [FromBody] ForecastRequest request,
    IForecastingService forecastingService,
    ILogger<Program> logger) =>
{
    try
    {
        request.Method = ForecastMethod.Manual;
        var result = await forecastingService.CalculateForecastAsync(request);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Manual forecast failed");
        return Results.BadRequest(new { Message = ex.Message });
    }
})
.WithName("ForecastManual")
.WithOpenApi();

// Forecast with method selection
app.MapPost("/api/forecast", async (
    [FromBody] ForecastRequest request,
    IForecastingService forecastingService,
    ForecastMethod? method,
    ILogger<Program> logger) =>
{
    try
    {
        var result = await forecastingService.CalculateForecastAsync(request, method);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Forecast failed");
        return Results.StatusCode(500);
    }
})
.WithName("Forecast")
.WithOpenApi();

// ============================================
// LLM Endpoints
// ============================================

// Generate LLM Completion
app.MapPost("/api/llm/completion", async (
    [FromBody] LLMRequest request,
    ILLMService llmService,
    ILogger<Program> logger) =>
{
    try
    {
        if (string.IsNullOrEmpty(request.Prompt))
        {
            return Results.BadRequest(new { Message = "Prompt is required" });
        }

        var result = await llmService.GenerateCompletionAsync(request);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "LLM completion failed");
        return Results.StatusCode(500);
    }
})
.WithName("LLMCompletion")
.WithOpenApi();

// Generate LLM Chat Completion
app.MapPost("/api/llm/chat", async (
    [FromBody] LLMRequest request,
    ILLMService llmService,
    ILogger<Program> logger) =>
{
    try
    {
        if (string.IsNullOrEmpty(request.Prompt) && (request.Messages == null || request.Messages.Count == 0))
        {
            return Results.BadRequest(new { Message = "Prompt or messages are required" });
        }

        var result = await llmService.GenerateChatCompletionAsync(request);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "LLM chat completion failed");
        return Results.StatusCode(500);
    }
})
.WithName("LLMChatCompletion")
.WithOpenApi();

// ============================================
// Skills Analysis Endpoints
// ============================================

// Analyze Skills Gap
app.MapPost("/api/skills/analyze", async (
    [FromBody] SkillsGapRequest request,
    ISkillsAnalysisService skillsAnalysisService,
    ILogger<Program> logger) =>
{
    try
    {
        if (request.RequiredSkills == null || request.RequiredSkills.Count == 0)
        {
            return Results.BadRequest(new { Message = "Required skills are required" });
        }

        if (request.CurrentSkills == null || request.CurrentSkills.Count == 0)
        {
            return Results.BadRequest(new { Message = "Current skills are required" });
        }

        var result = await skillsAnalysisService.AnalyzeSkillsGapAsync(request);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Skills gap analysis failed");
        return Results.StatusCode(500);
    }
})
.WithName("AnalyzeSkillsGap")
.WithOpenApi();

// Generate Skills Recommendations
app.MapPost("/api/skills/recommendations", async (
    [FromBody] SkillsGapResult gapAnalysis,
    ISkillsAnalysisService skillsAnalysisService,
    ILogger<Program> logger) =>
{
    try
    {
        var recommendations = await skillsAnalysisService.GenerateRecommendationsAsync(gapAnalysis);
        return Results.Ok(recommendations);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Skills recommendations generation failed");
        return Results.StatusCode(500);
    }
})
.WithName("GenerateSkillsRecommendations")
.WithOpenApi();

// ============================================
// API Information Endpoint
// ============================================

// API Methods Summary
app.MapGet("/api/info", () =>
{
    return Results.Ok(new
    {
        Title = "QFace.Sdk.AI - Demo API",
        Description = "This demo showcases all AI SDK capabilities",
        Endpoints = new
        {
            Forecasting = new[]
            {
                new { Method = "POST", Path = "/api/forecast/trend", Description = "Trend analysis forecasting" },
                new { Method = "POST", Path = "/api/forecast/regression", Description = "Regression analysis forecasting" },
                new { Method = "POST", Path = "/api/forecast/ml", Description = "ML.NET time series forecasting" },
                new { Method = "POST", Path = "/api/forecast/manual", Description = "Manual forecast with assumptions" },
                new { Method = "POST", Path = "/api/forecast", Description = "Forecast with method selection" }
            },
            LLM = new[]
            {
                new { Method = "POST", Path = "/api/llm/completion", Description = "Generate LLM completion" },
                new { Method = "POST", Path = "/api/llm/chat", Description = "Generate LLM chat completion" }
            },
            SkillsAnalysis = new[]
            {
                new { Method = "POST", Path = "/api/skills/analyze", Description = "Analyze skills gap" },
                new { Method = "POST", Path = "/api/skills/recommendations", Description = "Generate skills recommendations" }
            }
        },
        ForecastMethods = Enum.GetNames(typeof(ForecastMethod)),
        Usage = new
        {
            Forecasting = "Use POST /api/forecast with ForecastRequest containing HistoricalData and TargetDate",
            LLM = "Use POST /api/llm/completion or /api/llm/chat with LLMRequest containing Prompt",
            SkillsAnalysis = "Use POST /api/skills/analyze with SkillsGapRequest containing RequiredSkills and CurrentSkills"
        }
    });
})
.WithName("GetApiInfo")
.WithOpenApi();

app.Run();

// Make the implicit Program class public for testing
public partial class Program { }

