using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CivicConnect.IntegrationTests;

/// <summary>
/// Basis: S1-06 / S1-05: In-memory API startup and liveness verification.
/// Verifies that the Integration test project can bootstrap CivicConnect.Api in memory
/// using WebApplicationFactory without external service dependencies.
/// </summary>
public class ApiStartupTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiStartupTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Cors__Origins", "http://localhost:4200");
            builder.UseSetting("ConnectionStrings:Default", "Host=localhost;Port=5432;Database=TMT_civicconnect;Username=postgres;Password=CivicConnect123!");
        });
    }

    [Fact(DisplayName = "S1-06 / S1-05: API starts in memory and GET /health returns 200 Healthy")]
    public async Task GetHealth_WhenApiStartsInMemory_ReturnsOkWithHealthyStatus()
    {
        // Arrange
        // Basis: S1-05 response shape specification: { status: "Healthy", version, commit }
        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        Assert.Equal("Healthy", root.GetProperty("status").GetString());
        Assert.True(root.TryGetProperty("version", out _), "Response must contain 'version'");
        Assert.True(root.TryGetProperty("commit", out _), "Response must contain 'commit'");
    }
}