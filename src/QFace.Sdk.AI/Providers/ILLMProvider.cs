namespace QFace.Sdk.AI.Providers;

/// <summary>
/// Interface for LLM providers (OpenAI, Anthropic, Google Gemini, etc.)
/// </summary>
public interface ILLMProvider
{
    /// <summary>
    /// Provider name (e.g., "OpenAI", "Anthropic", "GoogleGemini")
    /// </summary>
    string ProviderName { get; }
    
    /// <summary>
    /// Initializes the provider with configuration
    /// </summary>
    Task<bool> InitializeAsync();
    
    /// <summary>
    /// Generates a completion from a prompt
    /// </summary>
    /// <param name="request">The LLM request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>LLM response</returns>
    Task<Models.LLMResponse> GenerateCompletionAsync(Models.LLMRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Generates a chat completion from messages
    /// </summary>
    /// <param name="request">The LLM request with messages</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>LLM response</returns>
    Task<Models.LLMResponse> GenerateChatCompletionAsync(Models.LLMRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if the provider is available and configured
    /// </summary>
    /// <returns>True if available, false otherwise</returns>
    Task<bool> IsAvailableAsync();
}

