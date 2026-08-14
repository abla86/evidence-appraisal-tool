using System.Net.Http.Json;
using System.Text.Json;
using EvidenceAppraisal.Api.Models;
using EvidenceAppraisal.Api.Services;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EvidenceAppraisal.Api.Tests;

public sealed class Amstar2ApiTests :
    IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public Amstar2ApiTests(
        WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Metadata_exposes_methodological_safeguards()
    {
        var response = await _client.GetAsync(
            "/api/amstar2/metadata"
        );

        response.EnsureSuccessStatusCode();

        var json = await response.Content
            .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            16,
            json.GetProperty("totalItems").GetInt32()
        );

        Assert.Contains(
            "must not be combined",
            json.GetProperty("scoringNotice")
                .GetString()
        );

        Assert.Contains(
            "must be prespecified",
            json.GetProperty("criticalDomainNotice")
                .GetString()
        );
    }

    [Fact]
    public async Task Valid_assessment_is_accepted_by_validation_endpoint()
    {
        var assessment = CreateValidAssessment();

        var response = await _client.PostAsJsonAsync(
            "/api/amstar2/validate",
            assessment
        );

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<ValidationResult>();

        Assert.NotNull(result);
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    private static Amstar2Assessment
        CreateValidAssessment()
   ()
    {
        int[] criticalItems =
        [
            2, 4, 7, 9, 11, 13, 15
        ];

        var criticalDomains = criticalItems
            .Select(item =>
                new CriticalDomainDefinition
                {
                    ItemNumber = item,
                    Rationale =
                        "Prespecified proposed critical domain from the AMSTAR 2 publication."
                })
            .ToArray();

        var items = Enumerable
            .Range(1, 16)
            .Select(item =>
                new Amstar2ItemAssessment
                {
                    ItemNumber = item,
                    Response = Amstar2Response.Yes,
                    Rationale =
                        "Documented test rationale.",
                    EvidenceLocation =
                        "Methods section, test location",
                    IsWeakness = false,
                    IsCriticalFlaw = false
                })
            .ToArray();

        return new Amstar2Assessment
        {
            ReviewTitle =
                "Example systematic review",
            Reviewer = "Test reviewer",
            CriticalDomains = criticalDomains,
            Items = items
        };
    }
}