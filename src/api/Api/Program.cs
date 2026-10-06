using CivicConnect.Data;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

// Configure pipeline
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// /health endpoint
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

app.Run();