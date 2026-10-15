using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

using CivicConnect.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Xunit;

namespace CivicConnect.IntegrationTests;

public class RequestsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public RequestsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Cors__Origins", "http://localhost:4200");
            builder.UseSetting("ConnectionStrings:Default",
                "Host=localhost;Port=5432;Database=TMT_civicconnect;Username=postgres;Password=CivicConnect123");
            builder.UseSetting("Jwt:Secret", Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(48)));
            builder.UseSetting("Jwt:Issuer", "CivicConnect.Tests");
            builder.UseSetting("Jwt:Audience", "CivicConnect.Tests");
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthenticationHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthenticationHandler.SchemeName;
                }).AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    TestAuthenticationHandler.SchemeName,
                    _ => { });
            });
        });

        _client = CreateRequesterClient(1);
    }

    [Fact(DisplayName = "FR-003/FR-004: POST /requests with valid input returns 201 with requestId and Submitted status")]
    public async Task SubmitRequest_WithValidInput_Returns201WithSubmittedStatus()
    {
        var body = new { categoryId = 1, description = "Integration test request" };

        var response = await _client.PostAsJsonAsync("/requests", body);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        Assert.True(root.GetProperty("requestId").GetInt64() > 0);
        Assert.Equal("Submitted", root.GetProperty("status").GetString());

        var secondResponse = await _client.PostAsJsonAsync("/requests", body);
        Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);
        var secondContent = await secondResponse.Content.ReadAsStringAsync();
        using var secondDoc = JsonDocument.Parse(secondContent);
        Assert.NotEqual(
            root.GetProperty("requestId").GetInt64(),
            secondDoc.RootElement.GetProperty("requestId").GetInt64());
    }

    [Fact(DisplayName = "FR-003: POST /requests with invalid category returns 422")]
    public async Task SubmitRequest_WithInvalidCategory_Returns422()
    {
        var body = new { categoryId = 99999, description = "Invalid category test" };

        var response = await _client.PostAsJsonAsync("/requests", body);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        Assert.True(doc.RootElement.GetProperty("errors").TryGetProperty("categoryId", out _));
    }

    [Fact(DisplayName = "FR-003: POST /requests with blank description returns 422 field error")]
    public async Task SubmitRequest_WithBlankDescription_Returns422FieldError()
    {
        var response = await _client.PostAsJsonAsync(
            "/requests",
            new { categoryId = 1, description = " " });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        Assert.True(doc.RootElement.GetProperty("errors").TryGetProperty("description", out _));
    }

    [Fact(DisplayName = "FR-007: GET /requests returns only the requester's own requests")]
    public async Task GetRequests_ReturnsOnlyOwnRequests()
    {
        var response = await _client.GetAsync("/requests");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        Assert.Equal(JsonValueKind.Array, root.ValueKind);

        // All returned requests must belong to requesterId 1 (placeholder)
        foreach (var item in root.EnumerateArray())
        {
            Assert.Equal(1, item.GetProperty("requesterId").GetInt64());
        }
    }

    [Fact(DisplayName = "FR-007: GET /requests/{id} with non-existent id returns 404 (isolation)")]
    public async Task GetRequestById_WithNonExistentId_Returns404()
    {
        var response = await _client.GetAsync("/requests/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(DisplayName = "FR-007: requester cannot view another requester's request")]
    public async Task GetRequestById_WithAnotherRequestersId_Returns404()
    {
        using var otherRequester = CreateRequesterClient(2);
        var postResponse = await otherRequester.PostAsJsonAsync(
            "/requests",
            new { categoryId = 1, description = "Isolation test request" });
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var postContent = await postResponse.Content.ReadAsStringAsync();
        using var postDoc = JsonDocument.Parse(postContent);
        var requestId = postDoc.RootElement.GetProperty("requestId").GetInt64();

        var response = await _client.GetAsync($"/requests/{requestId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(DisplayName = "FR-007: unauthenticated requester endpoints return 401")]
    public async Task Requests_WithoutAuthenticatedRequester_Returns401()
    {
        using var anonymousClient = _factory.CreateClient();

        var listResponse = await anonymousClient.GetAsync("/requests");
        var submitResponse = await anonymousClient.PostAsJsonAsync(
            "/requests",
            new { categoryId = 1, description = "Unauthenticated request" });

        Assert.Equal(HttpStatusCode.Unauthorized, listResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, submitResponse.StatusCode);
    }

    [Theory(DisplayName = "FR-007: request endpoints reject non-requester roles")]
    [InlineData(Roles.Staff)]
    [InlineData(Roles.Admin)]
    public async Task Requests_WithNonRequesterRole_Returns403(string role)
    {
        using var nonRequester = CreateRequesterClient(1, role);

        var listResponse = await nonRequester.GetAsync("/requests");
        var detailResponse = await nonRequester.GetAsync("/requests/999999");
        var submitResponse = await nonRequester.PostAsJsonAsync(
            "/requests",
            new { categoryId = 1, description = "Non-requester attempt" });

        Assert.Equal(HttpStatusCode.Forbidden, listResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, detailResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, submitResponse.StatusCode);
    }

    [Fact(DisplayName = "FR-005: GET /requests/{id} returns status name not status id")]
    public async Task GetRequestById_ReturnsStatusName()
    {
        // First submit a request
        var body = new { categoryId = 1, description = "Status name test" };
        var postResponse = await _client.PostAsJsonAsync("/requests", body);
        var postContent = await postResponse.Content.ReadAsStringAsync();
        using var postDoc = JsonDocument.Parse(postContent);
        var requestId = postDoc.RootElement.GetProperty("requestId").GetInt64();

        // Then get it
        var response = await _client.GetAsync($"/requests/{requestId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        Assert.Equal("Submitted", root.GetProperty("status").GetString());
    }

    [Fact(DisplayName = "FR-005: GET /requests/{id} derives Overdue from an expired due date")]
    public async Task GetRequestById_WithExpiredDueDate_ReturnsOverdueStatus()
    {
        var postResponse = await _client.PostAsJsonAsync(
            "/requests",
            new { categoryId = 1, description = "Overdue status test" });
        var postContent = await postResponse.Content.ReadAsStringAsync();
        using var postDoc = JsonDocument.Parse(postContent);
        var requestId = postDoc.RootElement.GetProperty("requestId").GetInt64();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var request = await db.ServiceRequests.SingleAsync(r => r.RequestId == requestId);
            request.SubmittedAt = DateTimeOffset.UtcNow.AddHours(-2);
            request.DueAt = DateTimeOffset.UtcNow.AddMinutes(-1);
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync($"/requests/{requestId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        Assert.Equal("Overdue", doc.RootElement.GetProperty("status").GetString());
    }

    private HttpClient CreateRequesterClient(long requesterId, string role = Roles.Requester)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Requester-Id", requesterId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Role", role);
        return client;
    }

    private sealed class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "Test";

        public TestAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("X-Test-Requester-Id", out var requesterId)
                || !long.TryParse(requesterId, out var parsedRequesterId)
                || parsedRequesterId <= 0)
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, parsedRequesterId.ToString()),
                new Claim("role", Request.Headers["X-Test-Role"].ToString())
            };
            var identity = new ClaimsIdentity(claims, SchemeName, ClaimTypes.NameIdentifier, "role");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, SchemeName);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}