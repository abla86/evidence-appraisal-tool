using EvidenceAppraisal.Api.Models;

namespace EvidenceAppraisal.Api.Services;

public sealed class GradeCertaintyService
{
    private static readonly IReadOnlyDictionary<string, (int Minimum, int Maximum)> AllowedDomains =
        new Dictionary<string, (int, int)>(StringComparer.OrdinalIgnoreCase)
        {
            ["Risk of bias"] = (-2, 0),
            ["Inconsistency"] = (-2, 0),
            ["Indirectness"] = (-2, 0),
            ["Imprecision"] = (-2, 0),
            ["Publication bias"] = (-1, 0),
            ["Large effect"] = (0, 2),
            ["Dose-response gradient"] = (0, 1),
            ["Plausible residual confounding"] = (0, 1)
        };

    public GradeOutcomeResult Evaluate(GradeOutcomeAssessment assessment)
    {
        var errors = Validate(assessment);
        var change = assessment.DomainJudgements?.Sum(d => d.LevelChange) ?? 0;
        var provisionalValue = Math.Clamp((int)assessment.InitialCertainty + change, 1, 4);
        var provisional = (GradeCertainty)provisionalValue;

        if (assessment.ReviewerConfirmedCertainty is not null &&
            assessment.ReviewerConfirmedCertainty != provisional &&
            string.IsNullOrWhiteSpace(assessment.FinalCertaintyRationale))
        {
            errors.Add("A departure from the provisional certainty requires an explicit rationale.");
        }

        return new GradeOutcomeResult
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            ProvisionalCertainty = provisional,
            NetLevelChange = change,
            RequiresReviewerConfirmation = assessment.ReviewerConfirmedCertainty is null
        };
    }

    private static List<string> Validate(GradeOutcomeAssessment assessment)
    {
        var errors = new List<string>();
        Required(assessment.OutcomeName, "Outcome name", errors);
        Required(assessment.Population, "Population", errors);
        Required(assessment.Intervention, "Intervention", errors);
        Required(assessment.Comparator, "Comparator", errors);
        Required(assessment.EffectMeasure, "Effect measure", errors);
        Required(assessment.InitialCertaintyRationale, "Initial certainty rationale", errors);
        Required(assessment.FinalCertaintyRationale, "Final certainty rationale", errors);

        if (assessment.Participants < 0) errors.Add("Participants cannot be negative.");
        if (assessment.Studies < 1) errors.Add("At least one study is required.");

        var judgements = assessment.DomainJudgements ?? [];
        foreach (var requiredDomain in AllowedDomains.Keys)
        {
            var matches = judgements.Where(d =>
                string.Equals(d.Domain, requiredDomain, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (matches.Length != 1)
                errors.Add($"Exactly one judgement is required for domain: {requiredDomain}.");
        }

        foreach (var judgement in judgements)
        {
            if (!AllowedDomains.TryGetValue(judgement.Domain, out var range))
            {
                errors.Add($"Unknown GRADE domain: {judgement.Domain}.");
                continue;
            }

            if (judgement.LevelChange < range.Minimum || judgement.LevelChange > range.Maximum)
                errors.Add($"{judgement.Domain}: level change must be {range.Minimum} to {range.Maximum}.");
            Required(judgement.Rationale, $"{judgement.Domain}: rationale", errors);
            Required(judgement.EvidenceLocation, $"{judgement.Domain}: evidence location", errors);
        }

        return errors;
    }

    private static void Required(string? value, string label, ICollection<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value)) errors.Add($"{label} is required.");
    }
}
