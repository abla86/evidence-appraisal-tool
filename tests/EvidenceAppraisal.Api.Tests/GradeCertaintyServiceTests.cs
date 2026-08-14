using EvidenceAppraisal.Api.Models;
using EvidenceAppraisal.Api.Services;

namespace EvidenceAppraisal.Api.Tests;

public sealed class GradeCertaintyServiceTests
{
    [Fact]
    public void OneSeriousDowngrade_FromHigh_ReturnsModerate()
    {
        var assessment = CreateAssessment() with
        {
            DomainJudgements = CreateJudgements(("Risk of bias", -1))
        };

        var result = new GradeCertaintyService().Evaluate(assessment);

        Assert.True(result.IsValid);
        Assert.Equal(GradeCertainty.Moderate, result.ProvisionalCertainty);
        Assert.Equal(-1, result.NetLevelChange);
    }

    [Fact]
    public void Certainty_IsClampedAtVeryLow()
    {
        var assessment = CreateAssessment() with
        {
            DomainJudgements = CreateJudgements(
                ("Risk of bias", -2),
                ("Inconsistency", -2),
                ("Indirectness", -2))
        };

        var result = new GradeCertaintyService().Evaluate(assessment);

        Assert.True(result.IsValid);
        Assert.Equal(GradeCertainty.VeryLow, result.ProvisionalCertainty);
    }

    private static GradeOutcomeAssessment CreateAssessment() => new()
    {
        OutcomeName = "Mortality",
        Importance = GradeOutcomeImportance.Critical,
        Population = "Adults",
        Intervention = "Intervention",
        Comparator = "Usual care",
        EffectMeasure = "Risk ratio",
        RelativeEffect = "RR 0.90",
        AbsoluteEffect = "10 fewer per 1000",
        Participants = 1000,
        Studies = 4,
        InitialCertainty = GradeCertainty.High,
        InitialCertaintyRationale = "Randomised evidence; starting level confirmed by reviewers",
        DomainJudgements = CreateJudgements(),
        ReviewerConfirmedCertainty = GradeCertainty.High,
        FinalCertaintyRationale = "All domains assessed"
    };

    private static IReadOnlyCollection<GradeDomainJudgement> CreateJudgements(
        params (string Domain, int Change)[] changes)
    {
        var selected = changes.ToDictionary(x => x.Domain, x => x.Change);
        var domains = new[]
        {
            "Risk of bias",
            "Inconsistency",
            "Indirectness",
            "Imprecision",
            "Publication bias",
            "Large effect",
            "Dose-response gradient",
            "Plausible residual confounding"
        };

        return domains.Select(domain => new GradeDomainJudgement
        {
            Domain = domain,
            LevelChange = selected.GetValueOrDefault(domain, 0),
            Rationale = "Assessed by reviewer",
            EvidenceLocation = "Evidence profile"
        }).ToArray();
    }
}
