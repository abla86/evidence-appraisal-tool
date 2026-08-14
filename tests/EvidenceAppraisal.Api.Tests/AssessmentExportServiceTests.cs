using System.IO.Compression;
using EvidenceAppraisal.Api.Models;
using EvidenceAppraisal.Api.Services;

namespace EvidenceAppraisal.Api.Tests;

public sealed class AssessmentExportServiceTests
{
    private readonly AssessmentExportService
        _service = new();

    private readonly AssessmentReport
        _report = CreateReport();

    [Fact]
    public void Word_export_is_valid_open_xml_package()
    {
        var file = _service.Create(
            _report,
            "word"
        );

        Assert.Equal("docx", file.Extension);
        Assert.True(file.Content.Length > 1000);

        using var archive =
            new ZipArchive(
                new MemoryStream(file.Content),
                ZipArchiveMode.Read
            );

        var documentEntry =
            archive.GetEntry(
                "word/document.xml"
            );

        Assert.NotNull(documentEntry);

        using var reader =
            new StreamReader(
                documentEntry!.Open()
            );

        var xml = reader.ReadToEnd();

        Assert.Contains(
            _report.ReviewTitle,
            xml
        );

        Assert.Contains(
            _report.Sha256,
            xml
        );
    }

    [Fact]
    public void Excel_export_is_valid_open_xml_package()
    {
        var file = _service.Create(
            _report,
            "excel"
        );

        Assert.Equal("xlsx", file.Extension);
        Assert.True(file.Content.Length > 1000);

        using var archive =
            new ZipArchive(
                new MemoryStream(file.Content),
                ZipArchiveMode.Read
            );

        var worksheet =
            archive.GetEntry(
                "xl/worksheets/sheet1.xml"
            );

        Assert.NotNull(worksheet);

        using var reader =
            new StreamReader(
                worksheet!.Open()
            );

        var xml = reader.ReadToEnd();

        Assert.Contains(
            _report.Sha256,
            xml
        );

        Assert.Contains(
            "Dokumentasjonssted",
            xml
        );
    }

    [Fact]
    public void Pdf_export_has_pdf_signature_and_content()
    {
        var file = _service.Create(
            _report,
            "pdf"
        );

        Assert.Equal("pdf", file.Extension);
        Assert.True(file.Content.Length > 1000);

        var signature =
            System.Text.Encoding.ASCII
                .GetString(
                    file.Content,
                    0,
                    5
                );

        Assert.Equal("%PDF-", signature);
    }

    [Fact]
    public void Unknown_export_format_is_rejected()
    {
        Assert.Throws<ArgumentException>(
            () => _service.Create(
                _report,
                "unknown"
            )
        );
    }

    private static AssessmentReport
        CreateReport()
    {
        var items = Enumerable
            .Range(1, 16)
            .Select(item =>
                new ReportItemAssessment
                {
                    ItemNumber = item,
                    Response = "Yes",
                    EvidenceLocation =
                        $"Methods, page {item}",
                    Rationale =
                        "Documented researcher judgement.",
                    IsWeakness = false,
                    IsCriticalFlaw = false,
                    WasPrespecifiedCriticalDomain =
                        item is 2 or 4 or 7 or 9
                            or 11 or 13 or 15
                })
            .ToArray();

        return new AssessmentReport
        {
            SchemaVersion = "1.0",
            AssessmentId = Guid.Parse(
                "83bbcf10-6ee9-4d7f-84f8-94776da692ac"
            ),
            InstrumentName = "AMSTAR 2",
            InstrumentVersion = "2017",
            ReviewTitle = "Test review",
            Reviewer = "Researcher 01",
            AssessmentDateUtc =
                DateTimeOffset.Parse(
                    "2026-08-14T01:00:00Z"
                ),
            CriticalDomains =
            [
                new ReportCriticalDomain
                {
                    ItemNumber = 2,
                    Rationale =
                        "Prespecified in protocol."
                }
            ],
            Items = items,
            WeaknessCount = 0,
            CriticalFlawCount = 0,
            FinalConfidence = "High",
            FinalConfidenceRationale =
                "Researcher-documented conclusion.",
            MethodologicalNotice =
                AssessmentReportFactory
                    .MethodologicalNotice,
            Sha256 = new string('a', 64)
        };
    }
}