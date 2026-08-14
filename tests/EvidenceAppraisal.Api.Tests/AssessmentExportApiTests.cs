using System.Net;
using System.Net.Http.Json;
using EvidenceAppraisal.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EvidenceAppraisal.Api.Tests;

public sealed class AssessmentExportApiTests :
    IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AssessmentExportApiTests(
        WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData(
        "word",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    )]
    [InlineData(
        "excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    )]
    [InlineData(
        "pdf",
        "application/pdf"
    )]
    public async Task Valid_assessment_exports_file(
        string format,
        string expectedContentType)
    {
        var response =
            await _client.PostAsJsonAsync(
                $"/api/amstar2/export/{format}",
                CreateAssessment()
            );

        response.EnsureSuccessStatusCode();

        Assert.Equal(
            expectedContentType,
            response.Content.Headers
                .ContentType?.MediaType
        );

        var content =
            await response.Content
                .ReadAsByteArrayAsync();

        Assert.True(content.Length > 1000);
    }

    [Fact]
    public async Task Incomplete_assessment_is_rejected()
    {
        var assessment = CreateAssessment()
            with
            {
                FinalConfidence = null,
                FinalConfidenceRationale = null
            };

        var response =
            await _client.PostAsJsonAsync(
                "/api/amstar2/export/word",
                assessment
            );

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode
        );
    }

    private static Amstar2Assessment
        CreateAssessment()
    {
        int[] criticalItems =
        [
            2, 4, 7, 9, 11, 13, 15
        ];

        return new Amstar2Assessment
        {
            ReviewTitle =
                "API export test",
            Reviewer =
                "Researcher 01",
            CriticalDomains =
                criticalItems
                    .Select(item =>
                        new CriticalDomainDefinition
                        {
                            ItemNumber = item,
                            Rationale =
                                "Prespecified in protocol."
                        })
                    .ToArray(),
            Items = Enumerable
                .Range(1, 16)
                .Select(item =>
                    new Amstar2ItemAssessment
                    {
                        ItemNumber = item,
                        Response =
                            Amstar2Response.Yes,
                        Rationale =
                            "Documented judgement.",
                        EvidenceLocation =
                            "Methods section",
                        IsWeakness = false,
                        IsCriticalFlaw = false
                    })
                .ToArray(),
            FinalConfidence =
                "High",
            FinalConfidenceRationale =
                "Researcher-documented conclusion."
        };
    }
}