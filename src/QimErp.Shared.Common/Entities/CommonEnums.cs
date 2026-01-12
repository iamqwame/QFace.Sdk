namespace QimErp.Shared.Common.Entities;

public enum RiskLevel
{
    Low,
    Medium,
    High,
    Critical
}

public enum RiskCategory
{
    Operational,
    Financial,
    Strategic,
    Compliance,
    Reputational,
    HealthSafety,
    Technology,
    HumanResource
}
public enum DisciplinaryCaseType
{
    Performance,
    Attendance,
    Misconduct,
    PolicyViolation,
    SafetyViolation
}
public enum DisciplinaryCaseSeverity
{
    Minor,
    Moderate,
    Serious,
    Gross
}
public enum HealthIssueType
{
    Injury,
    Illness,
    Accommodation,
    Screening,
    Emergency
}

public enum HealthIssueStatus
{
    Open,
    UnderTreatment,
    Resolved,
    Closed
}

