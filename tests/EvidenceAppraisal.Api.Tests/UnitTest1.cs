using EvidenceAppraisal.Api.Models;
using EvidenceAppraisal.Api.Services;

namespace EvidenceAppraisal.Api.Tests;

public sealed class Amstar2ValidationServiceTests
{
    private readonly Amstar2ValidationService _service
        = new();

    [Fact]
    public void Complete_documented_assessment_is_valid()
    {
        var result = _service.Validate(
            CreateValidAssessment()
        );

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Missing_item_is_invalid()
    {
        var assessment = CreateValidAssessment();

        assessment = assessment with
        {
            Items = assessment.Items
                .Where(item => item.ItemNumber != 16)
                .ToArray()
        };

        var result = _service.Validate(assessment);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.Contains("Missing: 16")
        );
    }

    [Fact]
    public void Duplicate_item_is_invalid()
    {
        var assessment = CreateValidAssessment();

        assessment = assessment with
        {
            Items =
            [
                .. assessment.Items,
                assessment.Items.First()
            ]
        };

        var result = _service.Validate(assessment);

        Assert.False(result.IsValid);
        Assert.Contains(
            "Each AMSTAR 2 item may only be assessed once.",
            result.Errors
        );
    }

    [Fact]
    public void Missing_rationale_is_invalid()
    {
        var assessment = CreateValidAssessment();

        assessment = assessment with
        {
            Items = assessment.Items
                .Select(item =>
                    item.ItemNumber == 4
                        ? item with { Rationale = "" }
                        : item)
                .ToArray()
        };

        var result = _service.Validate(assessment);

        Assert.False(result.IsValid);
        Assert.Contains(
            "Item 4 requires a rationale.",
            result.Errors
        );
    }

    [Fact]
    public void No_meta_analysis_response_is_restricted()
    {
        var assessment = CreateValidAssessment();

        assessment = assessment with
        {
            Items = assessment.Items
                .Select(item =>
                    item.ItemNumber == 5
                        ? item with
                        {
                            Response =
                                Amstar2Response
                                    .NoMetaAnalysisConducted
                        }
                        : item)
                .ToArray()
        };

        var result = _service.Validate(assessment);

        Assert.False(result.IsValid);
        Assert.Contains(
            "NoMetaAnalysisConducted is not valid for item 5.",
            result.Errors
        );
    }

    [Fact]
    public void Critical_flaw_requires_prespecified_domain()
    {
        var assessment = CreateValidAssessment();

        assessment = assessment with
        {
            Items = assessment.Items
                .Select(item =>
                    item.ItemNumber == 1
                        ? item with
                        {
                            IsWeakness = true,
                            IsCriticalFlaw = true
                        }
                        : item)
                .ToArray()
        };

        var result = _service.Validate(assessment);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.Contains(
                "was not prespecified as a critical domain"
            )
        );
    }

    [Fact]
    public void Final_confidence_requires_rationale()
    {
        var assessment = CreateValidAssessment()
            with
            {
                FinalConfidence = "Moderate",
                FinalConfidenceRationale = ""
            };

        var result = _service.Validate(assessment);

        Assert.False(result.IsValid);
        Assert.Contains(
            "A final confidence rating requires a documented rationale.",
            result.Errors
        );
    }

    private static Amstar2Assessment
        CreateValidAssessment()
    {
        int[] criticalItems =
        [
            2, 4, 7, 9, 11, 13, 15
        ];

        var criticalDomains = criticalItems
            .Select(item =>
                new CriticalDomainDefinition
                {
                    ItemNumber = item,
                    Rationale =
                        "Prespecified default critical domain from the AMSTAR 2 publication."
                })
            .ToArray();

        var items = Enumerable
            .Range(1, 16)
            .Select(item =>
                new Amstar2ItemAssessment
                {
                    ItemNumber = item,
                    Response = Amstar2Response.Yes,
                    Rationale =
                        "Test rationale documenting the judgement.",
                    EvidenceLocation =
                        "Methods section, test location",
                    IsWeakness = false,
                    IsCriticalFlaw = false
                })
            .ToArray();

        return new Amstar2Assessment
        {
            ReviewTitle = "Example systematic review",
            Reviewer = "Test reviewer",
            CriticalDomains = criticalDomains,
            Items = items
        };
    }
}