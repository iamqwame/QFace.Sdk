using QFace.Sdk.AI.Models;

namespace QFace.Sdk.AI.Services;

/// <summary>
/// Interface for skills analysis service
/// </summary>
public interface ISkillsAnalysisService
{
    /// <summary>
    /// Analyzes skills gap between required and current skills
    /// </summary>
    /// <param name="request">Skills gap analysis request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Skills gap analysis result</returns>
    Task<SkillsGapResult> AnalyzeSkillsGapAsync(SkillsGapRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Generates recommendations for addressing skills gaps
    /// </summary>
    /// <param name="gapAnalysis">Skills gap analysis result</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of recommendations</returns>
    Task<List<SkillsRecommendation>> GenerateRecommendationsAsync(SkillsGapResult gapAnalysis, CancellationToken cancellationToken = default);
}

