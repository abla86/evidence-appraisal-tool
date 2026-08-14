using EvidenceAppraisal.Api.Models;

namespace EvidenceAppraisal.Api.Services;

public sealed class CaspValidationService
{
    public CaspValidationResult Validate(CaspAssessment assessment)
    {
        var errors = new List<string>();

        Required(assessment.ChecklistTitle, "Checklist title", errors);
        Required(assessment.ChecklistVersion, "Checklist version", errors);
        Required(assessment.OfficialChecklistUrl, "Official checklist URL", errors);
        Required(assessment.StudyTitle, "Study title", errors);
        Required(assessment.ReviewerCode, "Reviewer code", errors);
        Required(assessment.OverallJudgement, "Overall judgement", errors);
        Required(assessment.OverallJudgementRationale, "Overall judgement rationale", errors);

        if (!Uri.TryCreate(assessment.OfficialChecklistUrl, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps)
            errors.Add("Official checklist URL must be an absolute HTTPS URL.");

        if (assessment.ExpectedItemCount is < 1 or > 100)
            errors.Add("Expected item count must be between 1 and 100.");

        var items = assessment.Items ?? [];
        var expected = Enumerable.Range(1, Math.Max(assessment.ExpectedItemCount, 0)).ToArray();
        var numbers = items.Select(i => i.ItemNumber).ToArray();

        if (numbers.Length != assessment.ExpectedItemCount ||
            numbers.Distinct().Count() != assessment.ExpectedItemCount ||
            !expected.All(numbers.Contains))
            errors.Add("The assessment must contain exactly one entry for every checklist item.");

        foreach (var item in items)
        {
            if (item.Response is null)
                errors.Add($"Item {item.ItemNumber}: response is required.");
            Required(item.Rationale, $"Item {item.ItemNumber}: rationale", errors);
            Required(item.EvidenceLocation, $"Item {item.ItemNumber}: evidence location", errors);
        }

        return new CaspValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            CompletedItems = items.Count(i =>
                i.Response is not null &&
                !string.IsNullOrWhiteSpace(i.Rationale) &&
                !string.IsNullOrWhiteSpace(i.EvidenceLocation)),
            ExpectedItems = assessment.ExpectedItemCount
        };
    }

    private static void Required(string? value, string label, ICollection<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
            errors.Add($"{label} is required.");
    }
}
