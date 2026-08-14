namespace EvidenceAppraisal.Api.Models;

public enum CaspResponse
{
    Yes,
    No,
    CannotTell
}

public sealed record CaspItemAssessment
{
    public required int ItemNumber { get; init; }
    public CaspResponse? Response { get; init; }
    public required string Rationale { get; init; }
    public required string EvidenceLocation { get; init; }
}

public sealed record CaspAssessment
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string InstrumentName { get; init; } = "CASP";
    public required string ChecklistTitle { get; init; }
    public required string ChecklistVersion { get; init; }
    public required string OfficialChecklistUrl { get; init; }
    public required int ExpectedItemCount { get; init; }
    public required string StudyTitle { get; init; }
    public required string ReviewerCode { get; init; }
    public DateTimeOffset AssessmentDateUtc { get; init; } = DateTimeOffset.UtcNow;
    public required IReadOnlyCollection<CaspItemAssessment> Items { get; init; }
    public required string OverallJudgement { get; init; }
    public required string OverallJudgementRationale { get; init; }
}

public sealed record CaspValidationResult
{
    public required bool IsValid { get; init; }
    public required IReadOnlyCollection<string> Errors { get; init; }
    public required int CompletedItems { get; init; }
    public required int ExpectedItems { get; init; }
}
