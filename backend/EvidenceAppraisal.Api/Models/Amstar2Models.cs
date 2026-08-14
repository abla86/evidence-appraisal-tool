namespace EvidenceAppraisal.Api.Models;

public enum Amstar2Response
{
    Yes,
    PartialYes,
    No,
    NoMetaAnalysisConducted
}

public sealed record CriticalDomainDefinition
{
    public required int ItemNumber { get; init; }
    public required string Rationale { get; init; }
}

public sealed record Amstar2ItemAssessment
{
    public required int ItemNumber { get; init; }

    public Amstar2Response? Response { get; init; }

    public required string Rationale { get; init; }

    public required string EvidenceLocation { get; init; }

    public bool? IsWeakness { get; init; }

    public bool? IsCriticalFlaw { get; init; }
}

public sealed record Amstar2Assessment
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string InstrumentName { get; init; } = "AMSTAR 2";

    public string InstrumentVersion { get; init; } = "2017";

    public required string ReviewTitle { get; init; }

    public required string Reviewer { get; init; }

    public DateTimeOffset AssessmentDateUtc { get; init; }
        = DateTimeOffset.UtcNow;

    public required IReadOnlyCollection<CriticalDomainDefinition>
        CriticalDomains { get; init; }

    public required IReadOnlyCollection<Amstar2ItemAssessment>
        Items { get; init; }

    public string? FinalConfidence { get; init; }

    public string? FinalConfidenceRationale { get; init; }
}