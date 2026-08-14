using EvidenceAppraisal.Api.Models;
using EvidenceAppraisal.Api.Services;

namespace EvidenceAppraisal.Api.Tests;

public sealed class Agree2ScoringServiceTests
{
    [Fact]
    public void AllRatingsAtSeven_ReturnsOneHundredPercentForEveryDomain()
    {
        var result = new Agree2ScoringService().Calculate(CreateAssessment(7));

        Assert.True(result.IsValid);
        Assert.Equal(6, result.DomainScores.Count);
        Assert.All(result.DomainScores, score =>
            Assert.Equal(100, score.StandardizedScorePercent));
    }

    [Fact]
    public void TwoIndependentAppraisers_MeetsMinimumFlag()
    {
        var assessment = CreateAssessment(4) with
        {
            Appraisers =
            [
                CreateAppraiser("R01", 4),
                CreateAppraiser("R02", 5)
            ]
        };

        var result = new Agree2ScoringService().Calculate(assessment);

        Assert.True(result.IsValid);
        Assert.True(result.IndependentAppraisalMinimumMet);
    }

    private static Agree2Assessment CreateAssessment(int score) => new()
    {
        GuidelineTitle = "Test guideline",
        GuidelineCitation = "Author (2026)",
        Appraisers = [CreateAppraiser("R01", score)]
    };

    private static Agree2AppraiserAssessment CreateAppraiser(string code, int score) => new()
    {
        AppraiserCode = code,
        Items = Enumerable.Range(1, 23).Select(number => new Agree2ItemRating
        {
            ItemNumber = number,
            Score = score,
            Rationale = "Documented rationale",
            EvidenceLocation = $"Page {number}"
        }).ToArray(),
        OverallQualityScore = score,
        Recommendation = "Recommend with modifications",
        RecommendationRationale = "Documented overall judgement"
    };
}
