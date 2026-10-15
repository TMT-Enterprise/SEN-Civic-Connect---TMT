using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

using Xunit;

namespace CivicConnect.IntegrationTests;

public class RequestsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public RequestsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Cors__Origins", "http://localhost:4200");
            builder.UseSetting("ConnectionStrings:Default",
                "Host=localhost;Port=5432;Database=TMT_civicconnect;Username=postgres;Password=CivicConnect123");
        }).CreateClient();
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
    }

    [Fact(DisplayName = "FR-003: POST /requests with invalid category returns 422")]
    public async Task SubmitRequest_WithInvalidCategory_Returns422()
    {
        var body = new { categoryId = 99999, description = "Invalid category test" };

        var response = await _client.PostAsJsonAsync("/requests", body);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
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
}