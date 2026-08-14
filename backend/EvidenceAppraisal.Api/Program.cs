using System.Text.Json.Serialization;
using EvidenceAppraisal.Api.Models;
using EvidenceAppraisal.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<Amstar2ValidationService>();
builder.Services.AddSingleton<AssessmentReportFactory>();
builder.Services.AddSingleton<AssessmentExportService>();
builder.Services.AddSingleton<Agree2ScoringService>();
builder.Services.AddSingleton<CaspValidationService>();
builder.Services.AddSingleton<GradeCertaintyService>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalReactFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("LocalReactFrontend");
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api", () => Results.Ok(new
{
    application = "Evidence Appraisal Tool API",
    status = "Research prototype",
    modules = new[] { "AMSTAR 2", "CASP", "AGREE II", "GRADE" },
    methodologicalNotice =
        "The API validates documented researcher judgements. It does not appraise evidence automatically or replace methodological expertise."
}));

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.MapGet("/api/instruments", () => Results.Ok(new object[]
{
    new
    {
        id = "amstar2",
        name = "AMSTAR 2",
        purpose = "Critical appraisal of systematic reviews of healthcare interventions",
        status = "Available",
        itemCount = 16,
        scoring = "No numerical total score"
    },
    new
    {
        id = "casp",
        name = "CASP",
        purpose = "Design-specific critical appraisal using an authorised CASP checklist",
        status = "Available",
        itemCount = (int?)null,
        scoring = "No automatic quality total"
    },
    new
    {
        id = "agree2",
        name = "AGREE II",
        purpose = "Appraisal of clinical practice guidelines",
        status = "Available",
        itemCount = 23,
        scoring = "Six standardised domain scores; no single required aggregate score"
    },
    new
    {
        id = "grade",
        name = "GRADE",
        purpose = "Outcome-level certainty of a body of evidence",
        status = "Available",
        itemCount = 8,
        scoring = "Four certainty categories with explicit domain judgements"
    }
}));

app.MapGet("/api/amstar2/metadata", () => Results.Ok(new
{
    instrumentName = "AMSTAR 2",
    instrumentVersion = "2017",
    totalItems = Amstar2ValidationService.TotalItems,
    proposedDefaultCriticalDomains = new[] { 2, 4, 7, 9, 11, 13, 15 },
    criticalDomainNotice =
        "The seven domains are proposed defaults from the original publication. Critical domains must be prespecified and justified for the appraisal context.",
    scoringNotice =
        "AMSTAR 2 item responses must not be combined into a numerical total score.",
    currentCapabilities = new[]
    {
        "Typed assessment submission",
        "Structural validation",
        "Required rationale validation",
        "Required evidence-location validation",
        "Critical-domain prespecification validation"
    },
    unavailableCapabilities = new[]
    {
        "Automatic professional judgement",
        "Data persistence",
        "Multi-reviewer reconciliation",
        "Clinical or policy recommendation"
    }
}));

app.MapPost("/api/amstar2/validate",
    (Amstar2Assessment assessment, Amstar2ValidationService service) =>
        Results.Ok(service.Validate(assessment)));

app.MapPost("/api/casp/validate",
    (CaspAssessment assessment, CaspValidationService service) =>
        Results.Ok(service.Validate(assessment)));

app.MapPost("/api/agree2/calculate",
    (Agree2Assessment assessment, Agree2ScoringService service) =>
        Results.Ok(service.Calculate(assessment)));

app.MapPost("/api/grade/evaluate",
    (GradeOutcomeAssessment assessment, GradeCertaintyService service) =>
        Results.Ok(service.Evaluate(assessment)));

app.MapPost("/api/amstar2/export/{format}",
    (
        string format,
        Amstar2Assessment assessment,
        Amstar2ValidationService validationService,
        AssessmentReportFactory reportFactory,
        AssessmentExportService exportService
    ) =>
    {
        var validation = validationService.Validate(assessment);
        if (!validation.IsValid) return Results.BadRequest(validation);

        try
        {
            var report = reportFactory.Create(assessment);
            var file = exportService.Create(report, format);
            return Results.File(
                file.Content,
                file.ContentType,
                $"amstar2-{assessment.Id}.{file.Extension}"
            );
        }
        catch (Exception exception) when (
            exception is ArgumentException or InvalidOperationException)
        {
            return Results.BadRequest(new { error = exception.Message });
        }
    });

app.MapFallbackToFile("index.html");
app.Run();

public partial class Program
{
}
