using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Fonts;
using S = DocumentFormat.OpenXml.Spreadsheet;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace EvidenceAppraisal.Api.Services;

public sealed record ExportedAssessmentFile(
    byte[] Content,
    string ContentType,
    string Extension
);

public sealed class AssessmentExportService
{
    private static readonly object
        PdfConfigurationLock = new();

    private static bool _pdfConfigured;

    public ExportedAssessmentFile Create(
        Models.AssessmentReport report,
        string format)
    {
        return format.ToLowerInvariant() switch
        {
            "word" => new ExportedAssessmentFile(
                CreateWord(report),
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "docx"
            ),

            "excel" => new ExportedAssessmentFile(
                CreateExcel(report),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "xlsx"
            ),

            "pdf" => new ExportedAssessmentFile(
                CreatePdf(report),
                "application/pdf",
                "pdf"
            ),

            _ => throw new ArgumentException(
                "Supported export formats are word, excel and pdf.",
                nameof(format)
            )
        };
    }

    public byte[] CreateWord(
        Models.AssessmentReport report)
    {
        using var stream = new MemoryStream();

        using (
            var document =
                WordprocessingDocument.Create(
                    stream,
                    WordprocessingDocumentType.Document,
                    true
                )
        )
        {
            var mainPart =
                document.AddMainDocumentPart();

            var body = new W.Body();

            body.Append(
                Heading(
                    "AMSTAR 2 – dokumentert kvalitetsvurdering",
                    32
                ),
                Paragraph(
                    $"Oversikt: {report.ReviewTitle}"
                ),
                Paragraph(
                    $"Vurderer: {report.Reviewer}"
                ),
                Paragraph(
                    $"Vurderingsdato (UTC): {report.AssessmentDateUtc:O}"
                ),
                Paragraph(
                    $"Samlet tillit, valgt av forskeren: {report.FinalConfidence}"
                ),
                Heading(
                    "Begrunnelse for samlet tillit",
                    24
                ),
                Paragraph(
                    report.FinalConfidenceRationale
                ),
                Heading(
                    "Forhåndsdefinerte kritiske domener",
                    24
                )
            );

            foreach (
                var domain in report.CriticalDomains)
            {
                body.Append(
                    Paragraph(
                        $"Punkt {domain.ItemNumber}: {domain.Rationale}"
                    )
                );
            }

            body.Append(
                Heading(
                    "Vurdering av de 16 punktene",
                    24
                )
            );

            foreach (var item in report.Items)
            {
                body.Append(
                    Heading(
                        $"Punkt {item.ItemNumber}",
                        22
                    ),
                    Paragraph(
                        $"Svar: {item.Response}"
                    ),
                    Paragraph(
                        $"Dokumentasjonssted: {item.EvidenceLocation}"
                    ),
                    Paragraph(
                        $"Begrunnelse: {item.Rationale}"
                    ),
                    Paragraph(
                        $"Metodisk svakhet: {YesNo(item.IsWeakness)}"
                    ),
                    Paragraph(
                        $"Kritisk svakhet: {YesNo(item.IsCriticalFlaw)}"
                    )
                );
            }

            body.Append(
                Heading(
                    "Etterprøvbarhet",
                    24
                ),
                Paragraph(
                    $"Rapportskjema: {report.SchemaVersion}"
                ),
                Paragraph(
                    $"Vurderings-ID: {report.AssessmentId}"
                ),
                Paragraph(
                    $"SHA-256: {report.Sha256}"
                ),
                Paragraph(
                    report.MethodologicalNotice
                )
            );

            mainPart.Document =
                new W.Document(body);

            mainPart.Document.Save();
        }

        return stream.ToArray();
    }

    public byte[] CreateExcel(
        Models.AssessmentReport report)
    {
        using var stream = new MemoryStream();

        using (
            var document =
                SpreadsheetDocument.Create(
                    stream,
                    SpreadsheetDocumentType.Workbook,
                    true
                )
        )
        {
            var workbookPart =
                document.AddWorkbookPart();

            workbookPart.Workbook =
                new S.Workbook();

            var worksheetPart =
                workbookPart.AddNewPart<
                    WorksheetPart
                >();

            var sheetData = new S.SheetData();

            sheetData.Append(
                Row(
                    "Punkt",
                    "Svar",
                    "Dokumentasjonssted",
                    "Faglig begrunnelse",
                    "Metodisk svakhet",
                    "Kritisk svakhet",
                    "Forhåndsdefinert kritisk domene"
                )
            );

            foreach (var item in report.Items)
            {
                sheetData.Append(
                    Row(
                        item.ItemNumber.ToString(),
                        item.Response,
                        item.EvidenceLocation,
                        item.Rationale,
                        YesNo(item.IsWeakness),
                        YesNo(item.IsCriticalFlaw),
                        YesNo(
                            item.WasPrespecifiedCriticalDomain
                        )
                    )
                );
            }

            sheetData.Append(
                Row(),
                Row(
                    "Samlet tillit",
                    report.FinalConfidence
                ),
                Row(
                    "Begrunnelse",
                    report.FinalConfidenceRationale
                ),
                Row(
                    "Vurderings-ID",
                    report.AssessmentId.ToString()
                ),
                Row(
                    "SHA-256",
                    report.Sha256
                ),
                Row(
                    "Metodisk merknad",
                    report.MethodologicalNotice
                )
            );

            worksheetPart.Worksheet =
                new S.Worksheet(sheetData);

            var sheets =
                workbookPart.Workbook.AppendChild(
                    new S.Sheets()
                );

            sheets.Append(
                new S.Sheet
                {
                    Id =
                        workbookPart.GetIdOfPart(
                            worksheetPart
                        ),
                    SheetId = 1,
                    Name = "AMSTAR 2-vurdering"
                }
            );

            workbookPart.Workbook.Save();
        }

        return stream.ToArray();
    }

    public byte[] CreatePdf(
        Models.AssessmentReport report)
    {
        ConfigurePdf();

        var document = new Document();

        document.Info.Title =
            $"AMSTAR 2 – {report.ReviewTitle}";

        document.Info.Author =
            report.Reviewer;

        var normal =
            document.Styles["Normal"];

        normal!.Font.Name = "Arial";
        normal.Font.Size = 9;

        var section = document.AddSection();

        AddHeading(
            section,
            "AMSTAR 2 – dokumentert kvalitetsvurdering",
            16
        );

        AddText(
            section,
            $"Oversikt: {report.ReviewTitle}"
        );

        AddText(
            section,
            $"Vurderer: {report.Reviewer}"
        );

        AddText(
            section,
            $"Samlet tillit, valgt av forskeren: {report.FinalConfidence}"
        );

        AddHeading(
            section,
            "Begrunnelse for samlet tillit",
            12
        );

        AddText(
            section,
            report.FinalConfidenceRationale
        );

        AddHeading(
            section,
            "Vurdering av de 16 punktene",
            12
        );

        foreach (var item in report.Items)
        {
            AddHeading(
                section,
                $"Punkt {item.ItemNumber}",
                10
            );

            AddText(
                section,
                $"Svar: {item.Response}"
            );

            AddText(
                section,
                $"Dokumentasjonssted: {item.EvidenceLocation}"
            );

            AddText(
                section,
                $"Begrunnelse: {item.Rationale}"
            );

            AddText(
                section,
                $"Metodisk svakhet: {YesNo(item.IsWeakness)}. " +
                $"Kritisk svakhet: {YesNo(item.IsCriticalFlaw)}."
            );
        }

        AddHeading(
            section,
            "Etterprøvbarhet",
            12
        );

        AddText(
            section,
            $"Vurderings-ID: {report.AssessmentId}"
        );

        AddText(
            section,
            $"SHA-256: {report.Sha256}"
        );

        AddText(
            section,
            report.MethodologicalNotice
        );

        var renderer =
            new PdfDocumentRenderer
            {
                Document = document
            };

        renderer.RenderDocument();

        using var stream =
            new MemoryStream();

        renderer.PdfDocument.Save(
            stream,
            false
        );

        return stream.ToArray();
    }

    private static W.Paragraph Paragraph(
        string text)
    {
        return new W.Paragraph(
            new W.Run(
                new W.Text(text)
                {
                    Space =
                        SpaceProcessingModeValues.Preserve
                }
            )
        );
    }

    private static W.Paragraph Heading(
        string text,
        int size)
    {
        return new W.Paragraph(
            new W.Run(
                new W.RunProperties(
                    new W.Bold(),
                    new W.FontSize
                    {
                        Val = size.ToString()
                    }
                ),
                new W.Text(text)
            )
        );
    }

    private static S.Row Row(
        params string[] values)
    {
        var row = new S.Row();

        foreach (var value in values)
        {
            row.Append(
                new S.Cell
                {
                    DataType =
                        S.CellValues.InlineString,
                    InlineString =
                        new S.InlineString(
                            new S.Text(value)
                        )
                }
            );
        }

        return row;
    }

    private static void AddHeading(
        Section section,
        string text,
        double size)
    {
        var paragraph =
            section.AddParagraph(text);

        paragraph.Format.Font.Bold = true;
        paragraph.Format.Font.Size = size;
        paragraph.Format.SpaceBefore = 8;
        paragraph.Format.SpaceAfter = 4;
    }

    private static void AddText(
        Section section,
        string text)
    {
        var paragraph =
            section.AddParagraph(text);

        paragraph.Format.SpaceAfter = 3;
    }

    private static string YesNo(bool value)
    {
        return value ? "Ja" : "Nei";
    }

    private static void ConfigurePdf()
    {
        lock (PdfConfigurationLock)
        {
            if (_pdfConfigured)
            {
                return;
            }

            if (OperatingSystem.IsWindows())
            {
                GlobalFontSettings
                    .UseWindowsFontsUnderWindows = true;
            }

            _pdfConfigured = true;
        }
    }
}