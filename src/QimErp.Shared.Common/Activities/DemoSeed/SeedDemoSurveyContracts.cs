using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities.DemoSeed;

/// <summary>
/// Surveys-side child workflow that seeds a comprehensive demo dataset
/// (surveys, questions, audiences, channels, schedules, participants,
/// responses, answers, analytics snapshots, share links) so every page
/// on the HR Operations Surveys sidebar has realistic content.
///
/// Owned by qimerp-hroperations-surveys-demo-seed task queue. May be invoked
/// stand-alone (admin endpoint POST /hr/surveys/admin/demo-seed) or as a
/// child of a platform DemoTenantSeedWorkflow.
/// </summary>
[Workflow("SeedDemoSurveyWorkflow")]
public interface ISeedDemoSurveyWorkflow
{
    [WorkflowRun]
    Task<SeedDemoSurveyResult> RunAsync(SeedDemoSurveyRequest request);
}

public sealed class SeedDemoSurveyRequest
{
    public required string TenantId { get; set; }
    public bool DryRun { get; set; }
    public bool AllowInProduction { get; set; }

    /// <summary>If true, removes previously seeded DEMO-* rows before reseeding.</summary>
    public bool Force { get; set; }

    /// <summary>System identity used to attribute seeded rows' CreatedBy / LastModifiedBy.</summary>
    public string SystemUserId { get; set; } = "demo-seed-system";
    public string SystemUserEmail { get; set; } = "demo-seed@qimerp.com";
    public string SystemUserName { get; set; } = "DemoSeedOrchestrator";
}

public sealed class SeedDemoSurveyResult
{
    public bool Success { get; set; }
    public bool AlreadySeeded { get; set; }
    public int SurveysCreated { get; set; }
    public int QuestionsCreated { get; set; }
    public int QuestionLogicCreated { get; set; }
    public int AudiencesCreated { get; set; }
    public int ChannelsCreated { get; set; }
    public int SchedulesCreated { get; set; }
    public int ParticipantsCreated { get; set; }
    public int ResponsesCreated { get; set; }
    public int AnswersCreated { get; set; }
    public int AnalyticsSnapshotsCreated { get; set; }
    public int Failed { get; set; }
    public List<string> Errors { get; } = new();
    public TimeSpan Elapsed { get; set; }
}
