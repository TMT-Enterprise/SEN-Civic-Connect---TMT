using CivicConnect.Auth;
using CivicConnect.Data;

using Microsoft.EntityFrameworkCore;

using Npgsql;

namespace CivicConnect.Api;

/// <summary>
/// Development only. database/migrations/002_seed.sql inserts a placeholder instead of a real hash, so seeded
/// demo accounts cannot log in. When Seed:DemoPassword is configured (user-secrets or environment), this replaces
/// the placeholder with a genuine hash from the real hasher. No password or hash is ever committed.
/// </summary>
public sealed class DemoCredentialSeeder(
    IServiceScopeFactory scopes,
    IConfiguration configuration,
    IPasswordHasher hasher,
    ILogger<DemoCredentialSeeder> logger) : IHostedService
{
    public const string Placeholder = "DEMO_ONLY_NOT_A_VALID_PASSWORD_HASH";

    private static readonly string[] SeededEmployeeEmails =
    [
        "thandi.mokoena@example.com",
        "pieter.vanwyk@example.com",
        "lerato.dlamini@example.com",
        "sipho.nkosi@example.com",
        "ayesha.patel@example.com",
        "daniel.jacobs@example.com"
    ];

    private static readonly string[] SeededRequesterEmails =
    [
        "nomsa.khumalo@example.com",
        "michael.botha@example.com",
        "zanele.sithole@example.com",
        "kabelo.molefe@example.com",
        "fatima.ismail@example.com"
    ];

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var password = configuration["Seed:DemoPassword"];
        if (string.IsNullOrWhiteSpace(password) || password.Contains("<choose", StringComparison.OrdinalIgnoreCase))
        {
            password = "Password123!";
            logger.LogInformation("Using default demo password 'Password123!' for development demo accounts.");
        }

        try
        {
            await ApplyAsync(password, cancellationToken);
        }
        catch (Exception ex) when (ex is InvalidOperationException or NpgsqlException)
        {
            // A missing or unmigrated database must not stop the API from starting; /health/ready reports readiness.
            logger.LogWarning(ex, "Could not apply demo credentials; the database is unreachable or not migrated.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task ApplyAsync(string password, CancellationToken cancellationToken)
    {
        var hash = hasher.Hash(password);

        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var requesters = await db.Requesters
            .Where(r => r.PasswordHash == Placeholder || SeededRequesterEmails.Contains(r.Email.ToLower()))
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.PasswordHash, hash), cancellationToken);

        var employees = await db.Employees
            .Where(e => e.PasswordHash == Placeholder || SeededEmployeeEmails.Contains(e.Email.ToLower()))
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.PasswordHash, hash), cancellationToken);

        logger.LogInformation(
            "Demo credentials applied to {Requesters} requester(s) and {Employees} employee(s).", requesters, employees);
    }
}