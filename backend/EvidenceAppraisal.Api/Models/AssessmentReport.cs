namespace EvidenceAppraisal.Api.Models;

public sealed record AssessmentReport
{
    public required string SchemaVersion { get; init; }

    public required Guid AssessmentId { get; init; }

    public required string InstrumentName { get; init; }

    public required string InstrumentVersion { get; init; }

    public required string ReviewTitle { get; init; }

    public required string Reviewer { get; init; }

    public required DateTimeOffset AssessmentDateUtc
    {
        get;
        init;
    }

    public required IReadOnlyCollection<
        ReportCriticalDomain
    > CriticalDomains { get; init; }

    public required IReadOnlyCollection<
        ReportItemAssessment
    > Items { get; init; }

    public required int WeaknessCount { get; init; }

    public required int CriticalFlawCount { get; init; }

    public required string FinalConfidence { get; init; }

    public required string FinalConfidenceRationale
    {
        get;
        init;
    }

    public required string MethodologicalNotice
    {
        get;
        init;
    }

    public required string Sha256 { get; init; }
}

public sealed record ReportCriticalDomain
{
    public required int ItemNumber { get; init; }

    public required string Rationale { get; init; }
}

public sealed record ReportItemAssessment
{
    public required int ItemNumber { get; init; }

    public required string Response { get; init; }

    public required string EvidenceLocation { get; init; }

    public required string Rationale { get; init; }

    public required bool IsWeakness { get; init; }

    public required bool IsCriticalFlaw { get; init; }

    public required bool WasPrespecifiedCriticalDomain
    {
        get;
        init;
    }
}