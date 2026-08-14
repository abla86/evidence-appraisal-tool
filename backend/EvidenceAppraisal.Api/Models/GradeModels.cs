namespace EvidenceAppraisal.Api.Models;

public enum GradeCertainty
{
    VeryLow = 1,
    Low = 2,
    Moderate = 3,
    High = 4
}

public enum GradeOutcomeImportance
{
    Important,
    Critical
}

public sealed record GradeDomainJudgement
{
    public required string Domain { get; init; }
    public required int LevelChange { get; init; }
    public required string Rationale { get; init; }
    public required string EvidenceLocation { get; init; }
}

public sealed record GradeOutcomeAssessment
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string OutcomeName { get; init; }
    public required GradeOutcomeImportance Importance { get; init; }
    public required string Population { get; init; }
    public required string Intervention { get; init; }
    public required string Comparator { get; init; }
    public required string EffectMeasure { get; init; }
    public required string RelativeEffect { get; init; }
    public required string AbsoluteEffect { get; init; }
    public required int Participants { get; init; }
    public required int Studies { get; init; }
    public required GradeCertainty InitialCertainty { get; init; }
    public required string InitialCertaintyRationale { get; init; }
    public required IReadOnlyCollection<GradeDomainJudgement> DomainJudgements { get; init; }
    public GradeCertainty? ReviewerConfirmedCertainty { get; init; }
    public required string FinalCertaintyRationale { get; init; }
}

public sealed record GradeOutcomeResult
{
    public required bool IsValid { get; init; }
    public required IReadOnlyCollection<string> Errors { get; init; }
    public required GradeCertainty ProvisionalCertainty { get; init; }
    public required int NetLevelChange { get; init; }
    public required bool RequiresReviewerConfirmation { get; init; }
}
