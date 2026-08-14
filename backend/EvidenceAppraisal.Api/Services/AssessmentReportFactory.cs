using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EvidenceAppraisal.Api.Models;

namespace EvidenceAppraisal.Api.Services;

public sealed class AssessmentReportFactory
{
    public const string SchemaVersion = "1.0";

    public const string MethodologicalNotice =
        "This report records researcher-entered AMSTAR 2 judgements. " +
        "It does not contain a numerical total score, does not infer " +
        "overall confidence automatically, and does not replace " +
        "professional critical appraisal.";

    private static readonly HashSet<string>
        AllowedConfidenceLevels =
    [
        "High",
        "Moderate",
        "Low",
        "CriticallyLow"
    ];

    private static readonly JsonSerializerOptions
        HashSerializerOptions = CreateHashOptions();

    public AssessmentReport Create(
        Amstar2Assessment assessment)
    {
        if (string.IsNullOrWhiteSpace(
                assessment.FinalConfidence))
        {
            throw new InvalidOperationException(
                "A researcher-selected final confidence rating is required for export."
            );
        }

        if (!AllowedConfidenceLevels.Contains(
                assessment.FinalConfidence))
        {
            throw new InvalidOperationException(
                "The final confidence rating is not an allowed AMSTAR 2 confidence category."
            );
        }

        if (string.IsNullOrWhiteSpace(
                assessment.FinalConfidenceRationale))
        {
            throw new InvalidOperationException(
                "A documented rationale for final confidence is required for export."
            );
        }

        var criticalDomains = assessment
            .CriticalDomains
            .OrderBy(domain => domain.ItemNumber)
            .Select(domain =>
                new ReportCriticalDomain
                {
                    ItemNumber =
                        domain.ItemNumber,

                    Rationale =
                        domain.Rationale.Trim()
                })
            .ToArray();

        var criticalNumbers = criticalDomains
            .Select(domain => domain.ItemNumber)
            .ToHashSet();

        var items = assessment.Items
            .OrderBy(item => item.ItemNumber)
            .Select(item =>
                new ReportItemAssessment
                {
                    ItemNumber =
                        item.ItemNumber,

                    Response =
                        item.Response?.ToString()
                        ?? throw new InvalidOperationException(
                            $"Item {item.ItemNumber} has no response."
                        ),

                    EvidenceLocation =
                        item.EvidenceLocation.Trim(),

                    Rationale =
                        item.Rationale.Trim(),

                    IsWeakness =
                        item.IsWeakness
                        ?? throw new InvalidOperationException(
                            $"Item {item.ItemNumber} has no weakness judgement."
                        ),

                    IsCriticalFlaw =
                        item.IsCriticalFlaw
                        ?? throw new InvalidOperationException(
                            $"Item {item.ItemNumber} has no critical-flaw judgement."
                        ),

                    WasPrespecifiedCriticalDomain =
                        criticalNumbers.Contains(
                            item.ItemNumber
                        )
                })
            .ToArray();

        var hashInput = new
        {
            SchemaVersion,
            assessment.Id,
            assessment.InstrumentName,
            assessment.InstrumentVersion,
            ReviewTitle =
                assessment.ReviewTitle.Trim(),
            Reviewer =
                assessment.Reviewer.Trim(),
            assessment.AssessmentDateUtc,
            CriticalDomains = criticalDomains,
            Items = items,
            FinalConfidence =
                assessment.FinalConfidence,
            FinalConfidenceRationale =
                assessment
                    .FinalConfidenceRationale
                    .Trim()
        };

        var sha256 = CalculateSha256(hashInput);

        return new AssessmentReport
        {
            SchemaVersion = SchemaVersion,
            AssessmentId = assessment.Id,
            InstrumentName =
                assessment.InstrumentName,
            InstrumentVersion =
                assessment.InstrumentVersion,
            ReviewTitle =
                assessment.ReviewTitle.Trim(),
            Reviewer =
                assessment.Reviewer.Trim(),
            AssessmentDateUtc =
                assessment.AssessmentDateUtc,
            CriticalDomains =
                criticalDomains,
            Items = items,
            WeaknessCount =
                items.Count(item =>
                    item.IsWeakness),
            CriticalFlawCount =
                items.Count(item =>
                    item.IsCriticalFlaw),
            FinalConfidence =
                assessment.FinalConfidence,
            FinalConfidenceRationale =
                assessment
                    .FinalConfidenceRationale
                    .Trim(),
            MethodologicalNotice =
                MethodologicalNotice,
            Sha256 = sha256
        };
    }

    private static string CalculateSha256(
        object value)
    {
        var json = JsonSerializer.Serialize(
            value,
            HashSerializerOptions
        );

        var bytes = Encoding.UTF8.GetBytes(json);
        var hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash)
            .ToLowerInvariant();
    }

    private static JsonSerializerOptions
        CreateHashOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy =
                JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        options.Converters.Add(
            new JsonStringEnumConverter()
        );

        return options;
    }
}