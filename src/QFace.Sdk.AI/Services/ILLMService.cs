using QFace.Sdk.AI.Models;

namespace QFace.Sdk.AI.Services;

/// <summary>
/// Interface for LLM service
/// </summary>
public interface ILLMService
{
    /// <summary>
    /// Generates a completion from a prompt
    /// </summary>
    /// <param name="request">LLM request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>LLM response</returns>
    Task<LLMResponse> GenerateCompletionAsync(LLMRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Generates a chat completion from messages
    /// </summary>
    /// <param name="request">LLM request with messages</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>LLM response</returns>
    Task<LLMResponse> GenerateChatCompletionAsync(LLMRequest request, CancellationToken cancellationToken = default);
}

