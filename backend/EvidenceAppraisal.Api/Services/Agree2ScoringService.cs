using EvidenceAppraisal.Api.Models;

namespace EvidenceAppraisal.Api.Services;

public sealed class Agree2ScoringService
{
    private static readonly (int Number, string Name, int[] Items)[] Domains =
    [
        (1, "Scope and purpose", [1, 2, 3]),
        (2, "Stakeholder involvement", [4, 5, 6]),
        (3, "Rigour of development", [7, 8, 9, 10, 11, 12, 13, 14]),
        (4, "Clarity of presentation", [15, 16, 17]),
        (5, "Applicability", [18, 19, 20, 21]),
        (6, "Editorial independence", [22, 23])
    ];

    public Agree2Result Calculate(Agree2Assessment assessment)
    {
        var errors = Validate(assessment);

        if (errors.Count > 0)
        {
            return new Agree2Result
            {
                IsValid = false,
                Errors = errors,
                DomainScores = [],
                AppraiserCount = assessment.Appraisers?.Count ?? 0,
                IndependentAppraisalMinimumMet = false
            };
        }

        var appraisers = assessment.Appraisers;
        var scores = Domains.Select(domain =>
        {
            var ratings = appraisers
                .SelectMany(a => a.Items)
                .Where(i => domain.Items.Contains(i.ItemNumber))
                .Select(i => i.Score)
                .ToArray();

            var minimum = domain.Items.Length * appraisers.Count;
            var maximum = domain.Items.Length * appraisers.Count * 7;
            var standardized = (ratings.Sum() - minimum) /
                (double)(maximum - minimum) * 100;

            return new Agree2DomainScore
            {
                DomainNumber = domain.Number,
                DomainName = domain.Name,
                ItemNumbers = domain.Items,
                StandardizedScorePercent = Math.Round(standardized, 1)
            };
        }).ToArray();

        return new Agree2Result
        {
            IsValid = true,
            Errors = [],
            DomainScores = scores,
            AppraiserCount = appraisers.Count,
            IndependentAppraisalMinimumMet = appraisers.Count >= 2
        };
    }

    private static List<string> Validate(Agree2Assessment assessment)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(assessment.GuidelineTitle))
            errors.Add("Guideline title is required.");
        if (string.IsNullOrWhiteSpace(assessment.GuidelineCitation))
            errors.Add("Guideline citation is required.");
        if (assessment.Appraisers is null || assessment.Appraisers.Count == 0)
        {
            errors.Add("At least one appraiser assessment is required.");
            return errors;
        }

        var duplicateCodes = assessment.Appraisers
            .Where(a => !string.IsNullOrWhiteSpace(a.AppraiserCode))
            .GroupBy(a => a.AppraiserCode.Trim(), StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        foreach (var code in duplicateCodes)
            errors.Add($"Appraiser code must be unique: {code}.");

        foreach (var appraiser in assessment.Appraisers)
        {
            if (string.IsNullOrWhiteSpace(appraiser.AppraiserCode))
                errors.Add("Every appraiser requires a pseudonymous appraiser code.");

            var itemNumbers = appraiser.Items?.Select(i => i.ItemNumber).ToArray() ?? [];
            var expected = Enumerable.Range(1, 23).ToArray();

            if (itemNumbers.Length != 23 || itemNumbers.Distinct().Count() != 23 ||
                !expected.All(itemNumbers.Contains))
                errors.Add($"Appraiser {appraiser.AppraiserCode}: exactly one rating for each item 1-23 is required.");

            foreach (var item in appraiser.Items ?? [])
            {
                if (item.Score is < 1 or > 7)
                    errors.Add($"Appraiser {appraiser.AppraiserCode}, item {item.ItemNumber}: score must be 1-7.");
                if (string.IsNullOrWhiteSpace(item.Rationale))
                    errors.Add($"Appraiser {appraiser.AppraiserCode}, item {item.ItemNumber}: rationale is required.");
                if (string.IsNullOrWhiteSpace(item.EvidenceLocation))
                    errors.Add($"Appraiser {appraiser.AppraiserCode}, item {item.ItemNumber}: evidence location is required.");
            }

            if (appraiser.OverallQualityScore is < 1 or > 7)
                errors.Add($"Appraiser {appraiser.AppraiserCode}: overall quality score must be 1-7.");
            if (string.IsNullOrWhiteSpace(appraiser.Recommendation))
                errors.Add($"Appraiser {appraiser.AppraiserCode}: recommendation is required.");
            if (string.IsNullOrWhiteSpace(appraiser.RecommendationRationale))
                errors.Add($"Appraiser {appraiser.AppraiserCode}: recommendation rationale is required.");
        }

        return errors;
    }
}
