namespace QFace.Sdk.AI.Models;

/// <summary>
/// Request for LLM completion generation
/// </summary>
public class LLMRequest
{
    /// <summary>
    /// Prompt text for completion
    /// </summary>
    public string Prompt { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional provider override (OpenAI, Anthropic, GoogleGemini)
    /// </summary>
    public string? Provider { get; set; }
    
    /// <summary>
    /// Optional model override
    /// </summary>
    public string? Model { get; set; }
    
    /// <summary>
    /// Optional max tokens override
    /// </summary>
    public int? MaxTokens { get; set; }
    
    /// <summary>
    /// Optional temperature override
    /// </summary>
    public double? Temperature { get; set; }
    
    /// <summary>
    /// Optional additional parameters
    /// </summary>
    public Dictionary<string, object>? Parameters { get; set; }
    
    /// <summary>
    /// Optional messages for chat-based models
    /// </summary>
    public List<LLMMessage>? Messages { get; set; }
}

/// <summary>
/// Message for chat-based LLM models
/// </summary>
public class LLMMessage
{
    /// <summary>
    /// Role of the message (system, user, assistant)
    /// </summary>
    public string Role { get; set; } = string.Empty;
    
    /// <summary>
    /// Content of the message
    /// </summary>
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// Response from LLM completion
/// </summary>
public class LLMResponse
{
    /// <summary>
    /// Generated content
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Provider that generated the response
    /// </summary>
    public string Provider { get; set; } = string.Empty;
    
    /// <summary>
    /// Model that generated the response
    /// </summary>
    public string Model { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of tokens used
    /// </summary>
    public int TokensUsed { get; set; }
    
    /// <summary>
    /// Optional metadata about the response
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

