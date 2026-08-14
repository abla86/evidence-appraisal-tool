using System.Text.Json.Serialization;
using EvidenceAppraisal.Api.Models;
using EvidenceAppraisal.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<Amstar2ValidationService>();
builder.Services.AddSingleton<AssessmentReportFactory>();
builder.Services.AddSingleton<AssessmentExportService>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter()
    );
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
    currentModule = "AMSTAR 2 validation",
    methodologicalNotice =
        "This API does not calculate a numerical AMSTAR 2 score or replace professional appraisal."
}));

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy"
}));

app.MapGet("/api/amstar2/metadata", () =>
    Results.Ok(new
    {
        instrumentName = "AMSTAR 2",
        instrumentVersion = "2017",
        totalItems = Amstar2ValidationService.TotalItems,

        proposedDefaultCriticalDomains =
            new[] { 2, 4, 7, 9, 11, 13, 15 },

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
            "Automatic overall-confidence calculation",
            "Data persistence",
            "Multi-reviewer reconciliation",
            "Clinical or policy recommendation"
        }
    })
);

app.MapPost(
    "/api/amstar2/validate",
    (
        Amstar2Assessment assessment,
        Amstar2ValidationService validationService
    ) =>
    {
        var validation = validationService.Validate(
            assessment
        );

        return Results.Ok(validation);
    }
);

app.MapPost(
    "/api/amstar2/export/{format}",
    (
        string format,
        Amstar2Assessment assessment,
        Amstar2ValidationService validationService,
        AssessmentReportFactory reportFactory,
        AssessmentExportService exportService
    ) =>
    {
        var validation = validationService.Validate(
            assessment
        );

        if (!validation.IsValid)
        {
            return Results.BadRequest(validation);
        }

        try
        {
            var report = reportFactory.Create(
                assessment
            );

            var file = exportService.Create(
                report,
                format
            );

            var fileName =
                $"amstar2-{assessment.Id}.{file.Extension}";

            return Results.File(
                file.Content,
                file.ContentType,
                fileName
            );
        }
        catch (
            Exception exception
        ) when (
            exception is ArgumentException or
            InvalidOperationException
        )
        {
            return Results.BadRequest(new
            {
                error = exception.Message
            });
        }
    }
);

app.MapFallbackToFile("index.html");

app.Run();

public partial class Program
{
}