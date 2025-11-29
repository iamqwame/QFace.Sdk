namespace QFace.Sdk.AI.Models;

/// <summary>
/// Configuration options for AI services
/// </summary>
public class AIOptions
{
    /// <summary>
    /// Default LLM provider to use (OpenAI, Anthropic, GoogleGemini)
    /// </summary>
    public string DefaultLLMProvider { get; set; } = "OpenAI";
    
    /// <summary>
    /// OpenAI configuration
    /// </summary>
    public OpenAIOptions OpenAI { get; set; } = new();
    
    /// <summary>
    /// Anthropic configuration
    /// </summary>
    public AnthropicOptions Anthropic { get; set; } = new();
    
    /// <summary>
    /// Google Gemini configuration
    /// </summary>
    public GoogleGeminiOptions GoogleGemini { get; set; } = new();
    
    /// <summary>
    /// Default forecasting method
    /// </summary>
    public ForecastMethod DefaultForecastMethod { get; set; } = ForecastMethod.Regression;
    
    /// <summary>
    /// Enable ML.NET forecasting (requires model training)
    /// </summary>
    public bool EnableMLForecasting { get; set; } = false;
    
    /// <summary>
    /// ML model storage path
    /// </summary>
    public string MLModelPath { get; set; } = "./ml-models";
}

/// <summary>
/// Configuration options for OpenAI provider
/// </summary>
public class OpenAIOptions
{
    /// <summary>
    /// OpenAI API key
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// OpenAI API base URL
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";
    
    /// <summary>
    /// Default model to use
    /// </summary>
    public string DefaultModel { get; set; } = "gpt-4";
    
    /// <summary>
    /// Maximum tokens to generate
    /// </summary>
    public int MaxTokens { get; set; } = 2000;
    
    /// <summary>
    /// Temperature for generation (0.0 to 2.0)
    /// </summary>
    public double Temperature { get; set; } = 0.7;
}

/// <summary>
/// Configuration options for Anthropic provider
/// </summary>
public class AnthropicOptions
{
    /// <summary>
    /// Anthropic API key
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Anthropic API base URL
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.anthropic.com/v1";
    
    /// <summary>
    /// Default model to use
    /// </summary>
    public string DefaultModel { get; set; } = "claude-3-5-sonnet-20241022";
    
    /// <summary>
    /// Maximum tokens to generate
    /// </summary>
    public int MaxTokens { get; set; } = 2000;
}

/// <summary>
/// Configuration options for Google Gemini provider
/// </summary>
public class GoogleGeminiOptions
{
    /// <summary>
    /// Google API key
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Google Gemini API base URL
    /// </summary>
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1";
    
    /// <summary>
    /// Default model to use
    /// </summary>
    public string DefaultModel { get; set; } = "gemini-pro";
    
    /// <summary>
    /// Maximum tokens to generate
    /// </summary>
    public int MaxTokens { get; set; } = 2000;
}

