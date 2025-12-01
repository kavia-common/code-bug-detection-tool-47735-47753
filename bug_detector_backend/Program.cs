using BugDetectorBackend.Middleware;
using BugDetectorBackend.Models;
using BugDetectorBackend.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using NSwag;

var builder = WebApplication.CreateBuilder(args);

// Service registration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "bug-detector-api";
    config.Title = "Bug Detector API";
    config.Version = "v1";
    config.Description = "Ocean Professional - Modern REST API to analyze code snippets and return detected issues.";
    // No explicit security configured; public preview.
    config.SchemaSettings.AllowReferencesWithProperties = true;
    config.PostProcess = document =>
    {
        document.Info.Contact = new OpenApiContact
        {
            Name = "Bug Detector",
            Email = "support@example.com",
            Url = "https://example.com"
        };
        document.Tags = new[]
        {
            new NSwag.OpenApiTag { Name = "Health", Description = "Service health and readiness" },
            new NSwag.OpenApiTag { Name = "Bugs", Description = "Code analysis and bug detection" }
        }.ToList();
    };
});

// Analyzer service (rule-based placeholder)
builder.Services.AddSingleton<IBugAnalyzer, RuleBasedBugAnalyzer>();

// CORS - permissive for preview environment
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowCredentials()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Pipeline
app.UseCors("AllowAll");
app.UseGlobalErrorHandling();

// OpenAPI/Swagger
app.UseOpenApi();
app.UseSwaggerUi(config =>
{
    config.Path = "/docs";
    config.DocumentTitle = "Bug Detector API - Docs";
});

// Health check endpoint (GET /health)
app.MapGet("/health", () =>
{
    // PUBLIC_INTERFACE
    // GET /health
    // Returns a simple status payload indicating the service is reachable.
    return Results.Ok(new { status = "healthy", name = "bug-detector-backend", version = "v1" });
})
.WithName("GetHealth")
.WithTags("Health");

// Root informational endpoint to mirror the previous behavior (GET /)
app.MapGet("/", () => Results.Ok(new { message = "Healthy" }))
    .WithName("GetRoot")
    .WithTags("Health");

// PUBLIC_INTERFACE
// POST /api/bugs/analyze
// Accepts BugAnalysisRequest and returns BugAnalysisResponse with detected issues.
app.MapPost("/api/bugs/analyze", async Task<Results<Ok<BugAnalysisResponse>, BadRequest<HttpValidationProblemDetails>>> (BugAnalysisRequest req, IBugAnalyzer analyzer, HttpContext httpContext, CancellationToken ct) =>
{
    // Minimal validation in addition to DataAnnotations
    if (string.IsNullOrWhiteSpace(req.Code))
    {
        var problem = new HttpValidationProblemDetails(new Dictionary<string, string[]>
        {
            ["code"] = new[] {"Code snippet is required."}
        });
        return TypedResults.BadRequest(problem);
    }

    var result = await analyzer.AnalyzeAsync(req, ct);
    return TypedResults.Ok(result);
})
.WithName("AnalyzeCode")
.WithTags("Bugs")
.Produces<BugAnalysisResponse>(StatusCodes.Status200OK)
.ProducesProblem(StatusCodes.Status400BadRequest);

app.Run();