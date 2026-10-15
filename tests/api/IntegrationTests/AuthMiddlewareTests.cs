using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;

using CivicConnect.Api;
using CivicConnect.Auth;
using CivicConnect.Data;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace CivicConnect.IntegrationTests;

public sealed class AuthMiddlewareTests : IAsyncLifetime
{
    private const string Issuer = "CivicConnect.Tests";
    private const string Audience = "CivicConnect.Tests";

    // Generated per run so no signing secret is ever committed (same approach as ApiFactory).
    private static readonly string Secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
    private static readonly string OtherSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));

    private WebApplication _app = default!;
    private HttpClient _client = default!;
    private ITokenService _tokens = default!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Secret"] = Secret,
            ["Jwt:Issuer"] = Issuer,
            ["Jwt:Audience"] = Audience,
            ["Jwt:ExpiryMinutes"] = "5"
        });
        builder.Services.AddAuthModule();
        builder.Services.AddApiAuthentication();

        _app = builder.Build();
        _app.UseAuthentication();
        _app.UseAuthorization();

        _app.MapGet("/probe/public", () => "ok").AllowAnonymous();
        _app.MapGet("/probe/unmarked", () => "ok"); // no metadata at all: protected by the fallback policy
        _app.MapGet("/probe/requester", () => "ok").RequireAuthorization(Policies.RequesterOnly);
        _app.MapGet("/probe/staff", () => "ok").RequireAuthorization(Policies.StaffOnly);
        _app.MapGet("/probe/admin", () => "ok").RequireAuthorization(Policies.AdminOnly);
        _app.MapGet("/probe/employee", () => "ok").RequireAuthorization(Policies.AnyEmployee);

        await _app.StartAsync();
        _tokens = _app.Services.GetRequiredService<ITokenService>();
        _client = _app.GetTestClient();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }

    private static AccountRecord AccountFor(string role) => new(
        1,
        role == Roles.Requester ? AccountTypes.Requester : AccountTypes.Employee,
        "Test",
        "test@example.com",
        "hash",
        role,
        true);

    private string TokenFor(string role) => _tokens.Create(AccountFor(role)).Token;

    private static string TokenFrom(string secret, TimeProvider clock, string role)
    {
        var options = new JwtOptions { Secret = secret, Issuer = Issuer, Audience = Audience, ExpiryMinutes = 5 };
        return new JwtTokenService(Options.Create(options), clock).Create(AccountFor(role)).Token;
    }

    private Task<HttpResponseMessage> GetAsync(string path, string? token = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        if (token is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return _client.SendAsync(request);
    }

    private static async Task AssertProblemAsync(HttpResponseMessage response, HttpStatusCode expected)
    {
        Assert.Equal(expected, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal((int)expected, body.GetProperty("status").GetInt32());
    }

    [Fact(DisplayName = "S2-04 / NF-001: an explicitly public route needs no token")]
    public async Task Public_route_is_reachable_without_a_token() =>
        Assert.Equal(HttpStatusCode.OK, (await GetAsync("/probe/public")).StatusCode);

    [Theory(DisplayName = "S2-04 / NF-001 / ASR-1: no token returns 401, including routes with no metadata")]
    [InlineData("/probe/unmarked")]
    [InlineData("/probe/requester")]
    [InlineData("/probe/staff")]
    [InlineData("/probe/admin")]
    [InlineData("/probe/employee")]
    public async Task No_token_returns_401_problem(string path) =>
        await AssertProblemAsync(await GetAsync(path), HttpStatusCode.Unauthorized);

    [Fact(DisplayName = "S2-04 / NF-001: a malformed token returns 401")]
    public async Task Malformed_token_returns_401() =>
        Assert.Equal(HttpStatusCode.Unauthorized, (await GetAsync("/probe/staff", "not.a.jwt")).StatusCode);

    [Fact(DisplayName = "S2-04 / NF-001: a token signed with another key returns 401")]
    public async Task Token_signed_with_another_key_returns_401() =>
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await GetAsync("/probe/staff", TokenFrom(OtherSecret, TimeProvider.System, Roles.Staff))).StatusCode);

    [Fact(DisplayName = "S2-04 / NF-001: an expired token returns 401")]
    public async Task Expired_token_returns_401() =>
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await GetAsync("/probe/staff", TokenFrom(Secret, new PastClock(), Roles.Staff))).StatusCode);

    [Theory(DisplayName = "S2-04 / NF-002: a valid token with the wrong role returns 403")]
    [InlineData(Roles.Requester, "/probe/staff")]
    [InlineData(Roles.Requester, "/probe/admin")]
    [InlineData(Roles.Requester, "/probe/employee")]
    [InlineData(Roles.Staff, "/probe/requester")]
    [InlineData(Roles.Staff, "/probe/admin")]
    [InlineData(Roles.Admin, "/probe/requester")]
    [InlineData(Roles.Admin, "/probe/staff")]
    public async Task Wrong_role_returns_403_problem(string role, string path) =>
        await AssertProblemAsync(await GetAsync(path, TokenFor(role)), HttpStatusCode.Forbidden);

    [Theory(DisplayName = "S2-04 / NF-002: a valid token with the right role reaches the handler")]
    [InlineData(Roles.Requester, "/probe/requester")]
    [InlineData(Roles.Staff, "/probe/staff")]
    [InlineData(Roles.Staff, "/probe/employee")]
    [InlineData(Roles.Admin, "/probe/admin")]
    [InlineData(Roles.Admin, "/probe/employee")]
    public async Task Correct_role_returns_200(string role, string path) =>
        Assert.Equal(HttpStatusCode.OK, (await GetAsync(path, TokenFor(role))).StatusCode);

    private sealed class PastClock : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => DateTimeOffset.UtcNow.AddHours(-2);
    }
}