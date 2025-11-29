using System.Text.Json;
using Microsoft.Extensions.Logging;
using QFace.Sdk.AI.Models;

namespace QFace.Sdk.AI.Services;

/// <summary>
/// Service for analyzing skills gaps and generating recommendations
/// </summary>
public class SkillsAnalysisService : ISkillsAnalysisService
{
    private readonly ILLMService _llmService;
    private readonly ILogger<SkillsAnalysisService> _logger;

    /// <summary>
    /// Initializes a new instance of SkillsAnalysisService
    /// </summary>
    public SkillsAnalysisService(ILLMService llmService, ILogger<SkillsAnalysisService> logger)
    {
        _llmService = llmService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<SkillsGapResult> AnalyzeSkillsGapAsync(SkillsGapRequest request, CancellationToken cancellationToken = default)
    {
        if (request.UseLLMForAnalysis)
        {
            return await AnalyzeWithLLMAsync(request, cancellationToken);
        }
        
        return AnalyzeWithRules(request);
    }

    /// <inheritdoc />
    public async Task<List<SkillsRecommendation>> GenerateRecommendationsAsync(
        SkillsGapResult gapAnalysis, 
        CancellationToken cancellationToken = default)
    {
        if (gapAnalysis.GapSkills.Count == 0)
        {
            return new List<SkillsRecommendation>();
        }

        var recommendations = new List<SkillsRecommendation>();

        foreach (var gapSkill in gapAnalysis.GapSkills)
        {
            var severity = gapAnalysis.GapSeverity.TryGetValue(gapSkill, out var s) ? s : "Medium";
            
            var recommendation = new SkillsRecommendation
            {
                Skill = gapSkill,
                RecommendationType = DetermineRecommendationType(gapSkill, severity),
                Description = GenerateRecommendationDescription(gapSkill, severity),
                Priority = severity
            };

            recommendations.Add(recommendation);
        }

        return recommendations;
    }

    private async Task<SkillsGapResult> AnalyzeWithLLMAsync(SkillsGapRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var prompt = BuildSkillsGapPrompt(request);
            var llmRequest = new LLMRequest
            {
                Prompt = prompt,
                Provider = request.LLMProvider
            };
            
            var response = await _llmService.GenerateCompletionAsync(llmRequest, cancellationToken);
            return ParseLLMResponse(response, request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing skills gap with LLM, falling back to rule-based analysis");
            return AnalyzeWithRules(request);
        }
    }

    private SkillsGapResult AnalyzeWithRules(SkillsGapRequest request)
    {
        var requiredSkills = request.RequiredSkills.Select(s => s.ToLowerInvariant()).ToList();
        var currentSkills = request.CurrentSkills.Select(s => s.ToLowerInvariant()).ToList();

        var matchedSkills = requiredSkills.Intersect(currentSkills).ToList();
        var gapSkills = requiredSkills.Except(currentSkills).ToList();

        var gapSeverity = new Dictionary<string, string>();
        foreach (var gapSkill in gapSkills)
        {
            // Simple rule: if skill contains "critical" or "essential", mark as Critical
            var severity = gapSkill.Contains("critical") || gapSkill.Contains("essential") 
                ? "Critical" 
                : gapSkill.Contains("important") 
                    ? "High" 
                    : "Medium";
            gapSeverity[gapSkill] = severity;
        }

        return new SkillsGapResult
        {
            GapSkills = gapSkills,
            MatchedSkills = matchedSkills,
            GapSeverity = gapSeverity,
            Recommendations = new List<SkillsRecommendation>()
        };
    }

    private string BuildSkillsGapPrompt(SkillsGapRequest request)
    {
        return $@"Analyze the skills gap between required and current skills.

Required Skills:
{string.Join("\n", request.RequiredSkills.Select((s, i) => $"{i + 1}. {s}"))}

Current Skills:
{string.Join("\n", request.CurrentSkills.Select((s, i) => $"{i + 1}. {s}"))}

Please provide:
1. List of missing skills (gaps)
2. List of matched skills
3. Severity for each gap (Critical, High, Medium, Low)
4. Recommendations for addressing gaps

Format your response as JSON with the following structure:
{{
  ""gapSkills"": [""skill1"", ""skill2""],
  ""matchedSkills"": [""skill1"", ""skill2""],
  ""gapSeverity"": {{""skill1"": ""Critical"", ""skill2"": ""High""}},
  ""recommendations"": [
    {{
      ""skill"": ""skill1"",
      ""recommendationType"": ""Training"",
      ""description"": ""..."",
      ""priority"": ""Critical""
    }}
  ]
}}";
    }

    private SkillsGapResult ParseLLMResponse(LLMResponse response, SkillsGapRequest request)
    {
        try
        {
            // Try to extract JSON from the response
            var content = response.Content;
            var jsonStart = content.IndexOf('{');
            var jsonEnd = content.LastIndexOf('}');
            
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var json = content.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var result = JsonSerializer.Deserialize<SkillsGapResult>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (result != null)
                {
                    return result;
                }
            }

            // Fallback to rule-based if JSON parsing fails
            _logger.LogWarning("Failed to parse LLM response as JSON, falling back to rule-based analysis");
            return AnalyzeWithRules(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing LLM response");
            return AnalyzeWithRules(request);
        }
    }

    private string DetermineRecommendationType(string skill, string severity)
    {
        // Simple rule-based recommendation type
        return severity switch
        {
            "Critical" => "Hiring",
            "High" => "Training",
            _ => "External"
        };
    }

    private string GenerateRecommendationDescription(string skill, string severity)
    {
        return severity switch
        {
            "Critical" => $"Immediate action required: {skill} is critical for operations. Consider hiring or external consulting.",
            "High" => $"High priority: {skill} is important. Recommend training program or certification.",
            "Medium" => $"Medium priority: {skill} would be beneficial. Consider online courses or workshops.",
            _ => $"Low priority: {skill} can be addressed through self-study or optional training."
        };
    }
}

