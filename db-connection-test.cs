using System;
using Microsoft.Extensions.Configuration;
using Npgsql;

var config = new ConfigurationBuilder()
    .SetBasePath(Environment.CurrentDirectory + @"\src\api\Api")
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.Development.json", optional: true)
    .AddUserSecrets<ProgramMarker>(optional: true)
    .AddEnvironmentVariables()
    .Build();

var cs = config.GetConnectionString("Default");

if (string.IsNullOrWhiteSpace(cs))
{
    Console.WriteLine("Connection string is missing.");
    return;
}

try
{
    await using var conn = new NpgsqlConnection(cs);
    await conn.OpenAsync();
    Console.WriteLine("SUCCESS: .NET connected to PostgreSQL.");
}
catch (Exception ex)
{
    Console.WriteLine($"FAILED: {ex.GetType().Name}: {ex.Message}");
}

public sealed class ProgramMarker { }
