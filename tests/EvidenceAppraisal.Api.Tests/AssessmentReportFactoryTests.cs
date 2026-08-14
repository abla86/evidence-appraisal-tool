using EvidenceAppraisal.Api.Models;
using EvidenceAppraisal.Api.Services;

namespace EvidenceAppraisal.Api.Tests;

public sealed class AssessmentReportFactoryTests
{
    private readonly AssessmentReportFactory
        _factory = new();

    [Fact]
    public void Complete_assessment_creates_report()
    {
        var assessment = CreateAssessment();

        var report = _factory.Create(
            assessment
        );

        Assert.Equal("1.0", report.SchemaVersion);
        Assert.Equal(
            assessment.Id,
            report.AssessmentId
        );
        Assert.Equal(16, report.Items.Count);
        Assert.Equal(1, report.WeaknessCount);
        Assert.Equal(1, report.CriticalFlawCount);
        Assert.Equal(
            "Moderate",
            report.FinalConfidence
        );
        Assert.Equal(64, report.Sha256.Length);
        Assert.Contains(
            "does not contain a numerical total score",
            report.MethodologicalNotice,
            StringComparison.OrdinalIgnoreCase
        );

        Assert.DoesNotContain(
            typeof(AssessmentReport)
                .GetProperties(),
            property =>
                property.Name.Equals(
                    "Score",
                    StringComparison.OrdinalIgnoreCase
                )
        );
    }

    [Fact]
    public void Same_assessment_creates_same_hash()
    {
        var assessment = CreateAssessment();

        var first = _factory.Create(
            assessment
        );

        var second = _factory.Create(
            assessment
        );

        Assert.Equal(
            first.Sha256,
            second.Sha256
        );
    }

    [Fact]
    public void Report_orders_items_by_item_number()
    {
        var assessment = CreateAssessment();

        var reversed = assessment with
        {
            Items = assessment.Items
                .Reverse()
                .ToArray()
        };

        var report = _factory.Create(
            reversed
        );

        Assert.Equal(
            Enumerable.Range(1, 16),
            report.Items.Select(
                item => item.ItemNumber
            )
        );
    }

    [Fact]
    public void Export_requires_researcher_confidence()
    {
        var assessment = CreateAssessment()
            with
            {
                FinalConfidence = null,
                FinalConfidenceRationale = null
            };

        var exception = Assert.Throws<
            InvalidOperationException
        >(
            () => _factory.Create(
                assessment
            )
        );

        Assert.Contains(
            "researcher-selected",
            exception.Message,
            StringComparison.OrdinalIgnoreCase
        );
    }

    [Fact]
    public void Export_rejects_unknown_confidence()
    {
        var assessment = CreateAssessment()
            with
            {
                FinalConfidence =
                    "AutomaticallyCalculated"
            };

        Assert.Throws<
            InvalidOperationException
        >(
            () => _factory.Create(
                assessment
            )
        );
    }

    private static Amstar2Assessment
        CreateAssessment()
    {
        int[] criticalItems =
        [
            2, 4, 7, 9, 11, 13, 15
        ];

        var criticalDomains =
            criticalItems
                .Select(item =>
                    new CriticalDomainDefinition
                    {
                        ItemNumber = item,
                        Rationale =
                            "Prespecified in the appraisal protocol."
                    })
                .ToArray();

        var items = Enumerable
            .Range(1, 16)
            .Select(item =>
                new Amstar2ItemAssessment
                {
                    ItemNumber = item,
                    Response =
                        item == 2
                            ? Amstar2Response.No
                            : Amstar2Response.Yes,
                    Rationale =
                        "Documented researcher judgement.",
                    EvidenceLocation =
                        "Methods section, page reference",
                    IsWeakness =
                        item == 2,
                    IsCriticalFlaw =
                        item == 2
                })
            .ToArray();

        return new Amstar2Assessment
        {
            Id = Guid.Parse(
                "a12b39a9-f354-42ba-95d5-7f5962d8a476"
            ),
            ReviewTitle =
                "Test systematic review",
            Reviewer =
                "Researcher 01",
            AssessmentDateUtc =
                DateTimeOffset.Parse(
                    "2026-08-14T01:00:00Z"
                ),
            CriticalDomains =
                criticalDomains,
            Items = items,
            FinalConfidence =
                "Moderate",
            FinalConfidenceRationale =
                "Researcher-documented conclusion based on the recorded weaknesses."
        };
    }
}