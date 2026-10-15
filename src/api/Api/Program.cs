using System.Reflection;

using CivicConnect.Api;
using CivicConnect.Auth;
using CivicConnect.Data;

using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Register data access and the Auth module (hashing, tokens, AuthService)
builder.Services.AddDataAccess(builder.Configuration);
builder.Services.AddAuthModule();
builder.Services.AddApiAuthentication();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHostedService<DemoCredentialSeeder>();
}

// CORS from config. "Cors:Origins" is what the Cors__Origins environment variable maps to;
// the literal "Cors__Origins" key is kept as a fallback for existing local setups.
var corsOrigins = (builder.Configuration["Cors:Origins"] ?? builder.Configuration["Cors__Origins"])?.Split(",")
    ?? throw new InvalidOperationException("Cors:Origins (Cors__Origins) is required.");

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Swagger in Development only
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

// Health checks
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("ConnectionStrings:Default is required.");

builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString,
        tags: new[] { "ready" });

var app = builder.Build();

// Configure pipeline
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapAuthEndpoints();

// /health — liveness only, no DB call
var version = Assembly.GetExecutingAssembly()
    .GetName()
    .Version?.ToString() ?? "unknown";

var commit = Environment.GetEnvironmentVariable("GIT_SHA") ?? "local";

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    version,
    commit
}));

// /health/ready — checks DB reachability
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    },
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                error = e.Value.Exception?.Message
            })
        });
        await context.Response.WriteAsync(result);
    }
});

app.Run();

// Lets the integration test project host the API in memory (WebApplicationFactory<Program>).
public partial class Program;