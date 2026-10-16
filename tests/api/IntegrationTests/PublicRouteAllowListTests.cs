using CivicConnect.Api;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace CivicConnect.IntegrationTests;

public sealed class PublicRouteAllowListTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly string[] AllowedAnonymous =
    [
        "/api/v1/auth/login",
        "/api/v1/auth/register",
        "/health",
        "/health/ready"
    ];

    private readonly WebApplicationFactory<Program> _factory;

    public PublicRouteAllowListTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Cors:Origins", "http://localhost:4200");
            builder.UseSetting("ConnectionStrings:Default", "Host=localhost;Port=5432;Database=TMT_civicconnect;Username=postgres;Password=tmt_org123");
            builder.UseSetting("Jwt:Secret", Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(48)));
            builder.UseSetting("Jwt:Issuer", "CivicConnect.Tests");
            builder.UseSetting("Jwt:Audience", "CivicConnect.Tests");
            builder.UseSetting("Overdue:Enabled", "false");
        });
    }

    [Fact(DisplayName = "S2-04 / NF-001 / ASR-1: only allow-listed routes are anonymous")]
    public void Only_allow_listed_routes_are_anonymous()
    {
        var anonymous = _factory.Services.GetRequiredService<EndpointDataSource>()
            .Endpoints
            .OfType<RouteEndpoint>()
            .Where(e => e.Metadata.GetMetadata<IAllowAnonymous>() is not null)
            .Select(e => e.RoutePattern.RawText!)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(AllowedAnonymous.Order(StringComparer.Ordinal), anonymous);
    }
}