using CivicConnect.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"Config connection string: {builder.Configuration.GetConnectionString("Default")}");

// Register data access
builder.Services.AddDataAccess(builder.Configuration);

// CORS from config
var corsOrigins = builder.Configuration["Cors__Origins"]?.Split(",")
    ?? throw new InvalidOperationException("Cors__Origins is required.");

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

    var csb = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);

    try
{
    await using var testConnection = new Npgsql.NpgsqlConnection(connectionString);
    await testConnection.OpenAsync();
    Console.WriteLine("DIRECT NPGSQL TEST: SUCCESS");
}
catch (Exception ex)
{
    Console.WriteLine($"DIRECT NPGSQL TEST: FAILED - {ex.Message}");
}

/* Console.WriteLine($"DB Host: {csb.Host}");
Console.WriteLine($"DB Port: {csb.Port}");
Console.WriteLine($"DB Name: {csb.Database}");
Console.WriteLine($"DB User: {csb.Username}");
Console.WriteLine($"DB Password supplied: {!string.IsNullOrEmpty(csb.Password)}");
Console.WriteLine($"DB Password length: {csb.Password?.Length}");
Console.WriteLine($"DB Connection String: {csb.ConnectionString}"); */

builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString,
        tags: new[] { "ready" });

var app = builder.Build();

// Configure pipeline
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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