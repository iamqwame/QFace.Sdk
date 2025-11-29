namespace QFace.Sdk.AI.Models;

/// <summary>
/// Request for skills gap analysis
/// </summary>
public class SkillsGapRequest
{
    /// <summary>
    /// List of required skills
    /// </summary>
    public List<string> RequiredSkills { get; set; } = new();
    
    /// <summary>
    /// List of current skills
    /// </summary>
    public List<string> CurrentSkills { get; set; } = new();
    
    /// <summary>
    /// Whether to use LLM for intelligent analysis
    /// </summary>
    public bool UseLLMForAnalysis { get; set; } = false;
    
    /// <summary>
    /// Optional LLM provider override
    /// </summary>
    public string? LLMProvider { get; set; }
}

/// <summary>
/// Result of skills gap analysis
/// </summary>
public class SkillsGapResult
{
    /// <summary>
    /// List of skills that are missing (gaps)
    /// </summary>
    public List<string> GapSkills { get; set; } = new();
    
    /// <summary>
    /// List of skills that are present (matched)
    /// </summary>
    public List<string> MatchedSkills { get; set; } = new();
    
    /// <summary>
    /// Severity of each gap skill (Critical, High, Medium, Low)
    /// </summary>
    public Dictionary<string, string> GapSeverity { get; set; } = new();
    
    /// <summary>
    /// Recommendations for addressing skills gaps
    /// </summary>
    public List<SkillsRecommendation> Recommendations { get; set; } = new();
    
    /// <summary>
    /// Optional metadata about the analysis
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Recommendation for addressing a skills gap
/// </summary>
public class SkillsRecommendation
{
    /// <summary>
    /// Skill that needs to be addressed
    /// </summary>
    public string Skill { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of recommendation (Training, Hiring, External)
    /// </summary>
    public string RecommendationType { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of the recommendation
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Priority of the recommendation (Critical, High, Medium, Low)
    /// </summary>
    public string Priority { get; set; } = string.Empty;
}

