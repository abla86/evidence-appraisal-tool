using EvidenceAppraisal.Api.Models;

namespace EvidenceAppraisal.Api.Services;

public sealed record ValidationResult(
    bool IsValid,
    IReadOnlyCollection<string> Errors
);

public sealed class Amstar2ValidationService
{
    public const int TotalItems = 16;

    private static readonly HashSet<int>
        NoMetaAnalysisResponseItems = [11, 12, 15];

    public ValidationResult Validate(
        Amstar2Assessment assessment)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(assessment.ReviewTitle))
        {
            errors.Add("Review title is required.");
        }

        if (string.IsNullOrWhiteSpace(assessment.Reviewer))
        {
            errors.Add("Reviewer is required.");
        }

        ValidateCriticalDomains(
            assessment.CriticalDomains,
            errors
        );

        ValidateItems(
            assessment.Items,
            assessment.CriticalDomains,
            errors
        );

        if (!string.IsNullOrWhiteSpace(
                assessment.FinalConfidence) &&
            string.IsNullOrWhiteSpace(
                assessment.FinalConfidenceRationale))
        {
            errors.Add(
                "A final confidence rating requires a documented rationale."
            );
        }

        return new ValidationResult(
            errors.Count == 0,
            errors
        );
    }

    private static void ValidateCriticalDomains(
        IReadOnlyCollection<CriticalDomainDefinition>?
            criticalDomains,
        ICollection<string> errors)
    {
        if (criticalDomains is null ||
            criticalDomains.Count == 0)
        {
            errors.Add(
                "Critical domains must be prespecified before appraisal."
            );

            return;
        }

        var invalidNumbers = criticalDomains
            .Where(domain =>
                domain.ItemNumber < 1 ||
                domain.ItemNumber > TotalItems)
            .Select(domain => domain.ItemNumber)
            .Distinct()
            .Order()
            .ToArray();

        if (invalidNumbers.Length > 0)
        {
            errors.Add(
                "Critical-domain item numbers must be between 1 and 16."
            );
        }

        var duplicates = criticalDomains
            .GroupBy(domain => domain.ItemNumber)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .Order()
            .ToArray();

        if (duplicates.Length > 0)
        {
            errors.Add(
                "Each critical domain may only be defined once."
            );
        }

        foreach (var domain in criticalDomains)
        {
            if (string.IsNullOrWhiteSpace(domain.Rationale))
            {
                errors.Add(
                    $"Critical domain {domain.ItemNumber} requires a rationale."
                );
            }
        }
    }

    private static void ValidateItems(
        IReadOnlyCollection<Amstar2ItemAssessment>? items,
        IReadOnlyCollection<CriticalDomainDefinition>?
            criticalDomains,
        ICollection<string> errors)
    {
        if (items is null)
        {
            errors.Add(
                "AMSTAR 2 item assessments are required."
            );

            return;
        }

        var duplicates = items
            .GroupBy(item => item.ItemNumber)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .Order()
            .ToArray();

        if (duplicates.Length > 0)
        {
            errors.Add(
                "Each AMSTAR 2 item may only be assessed once."
            );
        }

        var invalidNumbers = items
            .Where(item =>
                item.ItemNumber < 1 ||
                item.ItemNumber > TotalItems)
            .Select(item => item.ItemNumber)
            .Distinct()
            .Order()
            .ToArray();

        if (invalidNumbers.Length > 0)
        {
            errors.Add(
                "AMSTAR 2 item numbers must be between 1 and 16."
            );
        }

        var submittedNumbers = items
            .Select(item => item.ItemNumber)
            .ToHashSet();

        var missingNumbers = Enumerable
            .Range(1, TotalItems)
            .Where(number =>
                !submittedNumbers.Contains(number))
            .ToArray();

        if (missingNumbers.Length > 0)
        {
            errors.Add(
                $"All 16 items must be assessed. Missing: {string.Join(", ", missingNumbers)}."
            );
        }

        var criticalNumbers = criticalDomains?
            .Select(domain => domain.ItemNumber)
            .ToHashSet() ?? [];

        foreach (var item in items)
        {
            if (item.Response is null)
            {
                errors.Add(
                    $"Item {item.ItemNumber} requires a response."
                );
            }

            if (string.IsNullOrWhiteSpace(item.Rationale))
            {
                errors.Add(
                    $"Item {item.ItemNumber} requires a rationale."
                );
            }

            if (string.IsNullOrWhiteSpace(
                    item.EvidenceLocation))
            {
                errors.Add(
                    $"Item {item.ItemNumber} requires an evidence location or an explicit statement that information was not reported."
                );
            }

            if (item.IsWeakness is null)
            {
                errors.Add(
                    $"Item {item.ItemNumber} requires an explicit weakness judgement."
                );
            }

            if (item.IsCriticalFlaw is null)
            {
                errors.Add(
                    $"Item {item.ItemNumber} requires an explicit critical-flaw judgement."
                );
            }

            if (item.Response ==
                    Amstar2Response
                        .NoMetaAnalysisConducted &&
                !NoMetaAnalysisResponseItems.Contains(
                    item.ItemNumber))
            {
                errors.Add(
                    $"NoMetaAnalysisConducted is not valid for item {item.ItemNumber}."
                );
            }

            if (item.IsCriticalFlaw == true &&
                !criticalNumbers.Contains(item.ItemNumber))
            {
                errors.Add(
                    $"Item {item.ItemNumber} cannot be recorded as a critical flaw because it was not prespecified as a critical domain."
                );
            }

            if (item.IsCriticalFlaw == true &&
                item.IsWeakness != true)
            {
                errors.Add(
                    $"Item {item.ItemNumber} cannot be a critical flaw unless it is also recorded as a weakness."
                );
            }
        }
    }
}