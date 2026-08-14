using EvidenceAppraisal.Api.Models;
using EvidenceAppraisal.Api.Services;

namespace EvidenceAppraisal.Api.Tests;

public sealed class CaspValidationServiceTests
{
    [Fact]
    public void CompleteAuthorisedChecklistReference_IsValid()
    {
        var assessment = CreateAssessment(10);

        var result = new CaspValidationService().Validate(assessment);

        Assert.True(result.IsValid);
        Assert.Equal(10, result.CompletedItems);
    }

    [Fact]
    public void MissingEvidenceLocation_IsRejected()
    {
        var assessment = CreateAssessment(2) with
        {
            Items =
            [
                new CaspItemAssessment
                {
                    ItemNumber = 1,
                    Response = CaspResponse.Yes,
                    Rationale = "Rationale",
                    EvidenceLocation = ""
                },
                new CaspItemAssessment
                {
                    ItemNumber = 2,
                    Response = CaspResponse.No,
                    Rationale = "Rationale",
                    EvidenceLocation = "Page 4"
                }
            ]
        };

        var result = new CaspValidationService().Validate(assessment);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.Contains("evidence location", StringComparison.OrdinalIgnoreCase));
    }

    private static CaspAssessment CreateAssessment(int itemCount) => new()
    {
        ChecklistTitle = "CASP qualitative checklist",
        ChecklistVersion = "2024",
        OfficialChecklistUrl = "https://casp-uk.net/casp-tools-checklists/",
        ExpectedItemCount = itemCount,
        StudyTitle = "Test study",
        ReviewerCode = "R01",
        Items = Enumerable.Range(1, itemCount).Select(number => new CaspItemAssessment
        {
            ItemNumber = number,
            Response = CaspResponse.Yes,
            Rationale = "Documented rationale",
            EvidenceLocation = $"Page {number}"
        }).ToArray(),
        OverallJudgement = "Include",
        OverallJudgementRationale = "Relevant and sufficiently rigorous"
    };
}
