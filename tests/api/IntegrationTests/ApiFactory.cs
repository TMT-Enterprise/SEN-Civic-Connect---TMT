using System.Security.Cryptography;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

using Npgsql;

using Testcontainers.PostgreSql;

namespace CivicConnect.IntegrationTests;

public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string DemoPassword = "Demo-Passw0rd!";
    public const int ExpiryMinutes = 5;

    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:16").Build();

    // Generated per run so no signing secret is ever committed.
    private readonly string _jwtSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));

    public string ConnectionString => _database.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _database.StartAsync();

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        foreach (var file in Directory.GetFiles(FindMigrationsDirectory(), "*.sql").Order(StringComparer.Ordinal))
        {
            await using var command = new NpgsqlCommand(await File.ReadAllTextAsync(file), connection);
            await command.ExecuteNonQueryAsync();
        }
    }

    async Task IAsyncLifetime.DisposeAsync() => await _database.DisposeAsync();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development"); // runs the demo-credential seeder
        builder.UseSetting("ConnectionStrings:Default", ConnectionString);
        builder.UseSetting("Cors:Origins", "http://localhost:4200");
        builder.UseSetting("Jwt:Secret", _jwtSecret);
        builder.UseSetting("Jwt:Issuer", "CivicConnect.Tests");
        builder.UseSetting("Jwt:Audience", "CivicConnect.Tests");
        builder.UseSetting("Jwt:ExpiryMinutes", ExpiryMinutes.ToString(System.Globalization.CultureInfo.InvariantCulture));
        builder.UseSetting("Seed:DemoPassword", DemoPassword);
    }

    public async Task<T?> ScalarAsync<T>(string sql, params (string Name, object Value)[] parameters)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        var result = await command.ExecuteScalarAsync();
        return result is null or DBNull ? default : (T)result;
    }

    public async Task ExecuteAsync(string sql, params (string Name, object Value)[] parameters) =>
        await ScalarAsync<object>(sql, parameters);

    private static string FindMigrationsDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "CivicConnect.slnx")))
        {
            directory = directory.Parent;
        }

        return directory is null
            ? throw new InvalidOperationException("Could not locate CivicConnect.slnx to find database/migrations.")
            : Path.Combine(directory.FullName, "database", "migrations");
    }
}

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "api";
}