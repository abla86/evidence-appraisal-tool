namespace EvidenceAppraisal.Api.Models;

public sealed record Agree2ItemRating
{
    public required int ItemNumber { get; init; }
    public required int Score { get; init; }
    public required string Rationale { get; init; }
    public required string EvidenceLocation { get; init; }
}

public sealed record Agree2AppraiserAssessment
{
    public Guid AppraiserId { get; init; } = Guid.NewGuid();
    public required string AppraiserCode { get; init; }
    public required IReadOnlyCollection<Agree2ItemRating> Items { get; init; }
    public required int OverallQualityScore { get; init; }
    public required string Recommendation { get; init; }
    public required string RecommendationRationale { get; init; }
}

public sealed record Agree2Assessment
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string InstrumentName { get; init; } = "AGREE II";
    public string InstrumentVersion { get; init; } = "2017";
    public required string GuidelineTitle { get; init; }
    public required string GuidelineCitation { get; init; }
    public DateTimeOffset AssessmentDateUtc { get; init; } = DateTimeOffset.UtcNow;
    public required IReadOnlyCollection<Agree2AppraiserAssessment> Appraisers { get; init; }
}

public sealed record Agree2DomainScore
{
    public required int DomainNumber { get; init; }
    public required string DomainName { get; init; }
    public required IReadOnlyCollection<int> ItemNumbers { get; init; }
    public required double StandardizedScorePercent { get; init; }
}

public sealed record Agree2Result
{
    public required bool IsValid { get; init; }
    public required IReadOnlyCollection<string> Errors { get; init; }
    public required IReadOnlyCollection<Agree2DomainScore> DomainScores { get; init; }
    public required int AppraiserCount { get; init; }
    public required bool IndependentAppraisalMinimumMet { get; init; }
}
