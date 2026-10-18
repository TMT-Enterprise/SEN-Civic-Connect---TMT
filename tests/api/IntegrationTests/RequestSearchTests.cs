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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Xunit;

namespace CivicConnect.IntegrationTests;

[Collection(ApiCollection.Name)]
public sealed class RequestSearchTests : IDisposable
{
    private const string StaffEmail = "pieter.vanwyk@example.com";
    private const string OtherStaffEmail = "lerato.dlamini@example.com";
    private const string AdminEmail = "thandi.mokoena@example.com";

    private readonly ApiFactory _api;
    private readonly WebApplicationFactory<Program> _factory;

    public RequestSearchTests(ApiFactory api)
    {
        _api = api;
        _factory = api.WithWebHostBuilder(builder =>
        {
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
    }

    public void Dispose() => _factory.Dispose();

    [Fact(DisplayName = "FR-009: all search fields use case-insensitive partial matching")]
    public async Task SearchStaffRequests_MatchesPartialFilters()
    {
        var staffId = await EmployeeIdAsync(StaffEmail);
        var categoryId = await CategoryIdAsync("IT support");
        var requestId = await SubmitRequestAsync(categoryId);
        var dueAt = DateTimeOffset.UtcNow.AddDays(5);
        await AssignAsync(requestId, staffId, dueAt);

        using var staff = ClientFor(staffId, Roles.Staff);
        var url = $"/api/v1/staff/requests?id={requestId.ToString()[^2..]}"
            + "&status=ssig"
            + $"&dueDate={dueAt:yyyy-MM}"
            + "&category=it%20supp&assignee=PIETER";
        using var response = await staff.GetAsync(url);

        var content = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, content);
        using var document = JsonDocument.Parse(content);
        var results = document.RootElement.GetProperty("requests");
        Assert.Contains(
            results.EnumerateArray(),
            request => request.GetProperty("requestId").GetInt64() == requestId);
        Assert.Null(document.RootElement.GetProperty("message").GetString());

        using var globalSearch = await staff.GetAsync("/api/v1/staff/requests?search=PIET");
        using var globalSearchDocument = JsonDocument.Parse(
            await globalSearch.Content.ReadAsStringAsync());
        Assert.Contains(
            globalSearchDocument.RootElement.GetProperty("requests").EnumerateArray(),
            request => request.GetProperty("requestId").GetInt64() == requestId);

        using var globalDateSearch = await staff.GetAsync(
            $"/api/v1/staff/requests?search={dueAt:yyyy-MM}");
        using var globalDateSearchDocument = JsonDocument.Parse(
            await globalDateSearch.Content.ReadAsStringAsync());
        Assert.Contains(
            globalDateSearchDocument.RootElement.GetProperty("requests").EnumerateArray(),
            request => request.GetProperty("requestId").GetInt64() == requestId);
    }

    [Fact(DisplayName = "FR-009: staff results are scoped; admin can view and filter unassigned requests")]
    public async Task SearchRequests_EnforcesRoleScopeAndAdminFilters()
    {
        var staffId = await EmployeeIdAsync(StaffEmail);
        var otherStaffId = await EmployeeIdAsync(OtherStaffEmail);
        var adminId = await EmployeeIdAsync(AdminEmail);
        var categoryId = await CategoryIdAsync("Facility faults");

        var ownRequestId = await SubmitRequestAsync(categoryId);
        var otherRequestId = await SubmitRequestAsync(categoryId);
        var unassignedRequestId = await SubmitRequestAsync(categoryId);
        await AssignAsync(ownRequestId, staffId, DateTimeOffset.UtcNow.AddDays(5));
        await AssignAsync(otherRequestId, otherStaffId, DateTimeOffset.UtcNow.AddDays(5));

        using var staff = ClientFor(staffId, Roles.Staff);
        using var ownResponse = await staff.GetAsync("/api/v1/staff/requests");
        var ownContent = await ownResponse.Content.ReadAsStringAsync();
        Assert.True(ownResponse.StatusCode == HttpStatusCode.OK, ownContent);
        using var ownDocument = JsonDocument.Parse(ownContent);
        var ownResults = ownDocument.RootElement.GetProperty("requests").EnumerateArray().ToArray();
        Assert.Contains(ownResults, request => request.GetProperty("requestId").GetInt64() == ownRequestId);
        Assert.DoesNotContain(ownResults, request => request.GetProperty("requestId").GetInt64() == otherRequestId);
        Assert.DoesNotContain(ownResults, request => request.GetProperty("requestId").GetInt64() == unassignedRequestId);
        Assert.All(
            ownResults,
            request => Assert.Equal(
                staffId,
                request.GetProperty("assignedEmployeeId").GetInt64()));

        using var staffAdminView = await staff.GetAsync("/api/v1/admin/requests");
        Assert.Equal(HttpStatusCode.Forbidden, staffAdminView.StatusCode);

        using var admin = ClientFor(adminId, Roles.Admin);
        using var allResponse = await admin.GetAsync("/api/v1/admin/requests");
        Assert.Equal(HttpStatusCode.OK, allResponse.StatusCode);
        using var allDocument = JsonDocument.Parse(await allResponse.Content.ReadAsStringAsync());
        var allResults = allDocument.RootElement.GetProperty("requests").EnumerateArray().ToArray();
        Assert.Contains(allResults, request => request.GetProperty("requestId").GetInt64() == unassignedRequestId);
        Assert.Contains(allResults, request => request.GetProperty("requestId").GetInt64() == otherRequestId);

        using var unassignedResponse = await admin.GetAsync("/api/v1/admin/requests?unassigned=true");
        using var unassignedDocument = JsonDocument.Parse(await unassignedResponse.Content.ReadAsStringAsync());
        var unassignedResults = unassignedDocument.RootElement.GetProperty("requests").EnumerateArray();
        Assert.Contains(
            unassignedResults,
            request => request.GetProperty("requestId").GetInt64() == unassignedRequestId);

        using var assigneeResponse = await admin.GetAsync(
            $"/api/v1/admin/requests?assignedEmployeeId={staffId}");
        using var assigneeDocument = JsonDocument.Parse(await assigneeResponse.Content.ReadAsStringAsync());
        var assigneeResults = assigneeDocument.RootElement.GetProperty("requests").EnumerateArray();
        Assert.Contains(
            assigneeResults,
            request => request.GetProperty("requestId").GetInt64() == ownRequestId);
        Assert.DoesNotContain(
            assigneeResults,
            request => request.GetProperty("requestId").GetInt64() == otherRequestId);

        using var assigneeNameResponse = await admin.GetAsync("/api/v1/admin/requests?assignee=PIET");
        using var assigneeNameDocument = JsonDocument.Parse(
            await assigneeNameResponse.Content.ReadAsStringAsync());
        var assigneeNameResults = assigneeNameDocument.RootElement
            .GetProperty("requests")
            .EnumerateArray();
        Assert.Contains(
            assigneeNameResults,
            request => request.GetProperty("requestId").GetInt64() == ownRequestId);
        Assert.DoesNotContain(
            assigneeNameResults,
            request => request.GetProperty("requestId").GetInt64() == otherRequestId);
    }

    [Fact(DisplayName = "FR-009: empty query returns permitted rows and no matches include a helpful message")]
    public async Task SearchStaffRequests_EmptyAndZeroResultPartitions()
    {
        var staffId = await EmployeeIdAsync(StaffEmail);
        var requestId = await SubmitRequestAsync(await CategoryIdAsync("Other"));
        await AssignAsync(requestId, staffId, DateTimeOffset.UtcNow.AddDays(5));

        using var staff = ClientFor(staffId, Roles.Staff);
        using var allResponse = await staff.GetAsync("/api/v1/staff/requests");
        var allContent = await allResponse.Content.ReadAsStringAsync();
        Assert.True(allResponse.StatusCode == HttpStatusCode.OK, allContent);
        using var allDocument = JsonDocument.Parse(allContent);
        var allResults = allDocument.RootElement.GetProperty("requests").EnumerateArray().ToArray();
        Assert.Contains(allResults, request => request.GetProperty("requestId").GetInt64() == requestId);

        using var noneResponse = await staff.GetAsync("/api/v1/staff/requests?id=not-a-request-id");
        using var noneDocument = JsonDocument.Parse(await noneResponse.Content.ReadAsStringAsync());
        Assert.Empty(noneDocument.RootElement.GetProperty("requests").EnumerateArray());
        Assert.Equal(
            "No requests matched your filters.",
            noneDocument.RootElement.GetProperty("message").GetString());

        using var invalidDateResponse = await staff.GetAsync("/api/v1/staff/requests?dueDate=not-a-date");
        Assert.Equal(HttpStatusCode.BadRequest, invalidDateResponse.StatusCode);
    }

    [Fact(DisplayName = "NF-006: request search columns have supporting database indexes")]
    public async Task RequestSearch_IndexesExist()
    {
        var indexes = await _api.ScalarAsync<string[]>(
            """
            SELECT array_agg(indexname ORDER BY indexname)
            FROM pg_indexes
            WHERE (tablename, indexname) IN (
                ('service_request', 'ix_service_request_due_at'),
                ('service_request', 'ix_service_request_id_trgm'),
                ('service_request', 'service_request_pkey'),
                ('service_request', 'ix_request_status'),
                ('service_request', 'ix_request_category'),
                ('service_request', 'ix_request_assigned_employee'),
                ('request_status', 'ix_request_status_name_trgm'),
                ('service_category', 'ix_service_category_type_trgm'),
                ('employee', 'ix_employee_name_trgm')
            )
            """);

        Assert.NotNull(indexes);
        Assert.Equal(9, indexes.Length);
        Assert.Contains("ix_service_request_due_at", indexes);
        Assert.Contains("ix_service_request_id_trgm", indexes);
        Assert.Contains("service_request_pkey", indexes);
        Assert.Contains("ix_request_status", indexes);
        Assert.Contains("ix_request_category", indexes);
        Assert.Contains("ix_request_assigned_employee", indexes);
        Assert.Contains("ix_request_status_name_trgm", indexes);
        Assert.Contains("ix_service_category_type_trgm", indexes);
        Assert.Contains("ix_employee_name_trgm", indexes);
    }

    private HttpClient ClientFor(long userId, string role)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Requester-Id", userId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Role", role);
        return client;
    }

    private async Task<long> SubmitRequestAsync(long categoryId)
    {
        using var requester = ClientFor(1, Roles.Requester);
        using var response = await requester.PostAsJsonAsync(
            "/api/v1/requests",
            new { categoryId, description = $"S2-08 search test {Guid.NewGuid()}" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("requestId").GetInt64();
    }

    private async Task AssignAsync(long requestId, long employeeId, DateTimeOffset dueAt)
    {
        using var admin = ClientFor(await EmployeeIdAsync(AdminEmail), Roles.Admin);
        using var response = await admin.PostAsJsonAsync(
            $"/api/v1/requests/{requestId}/assign",
            new { employeeId, dueAt });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private Task<long> EmployeeIdAsync(string email) =>
        _api.ScalarAsync<long>(
            "SELECT employee_id FROM employee WHERE email = @email",
            ("email", email));

    private Task<long> CategoryIdAsync(string category) =>
        _api.ScalarAsync<long>(
            "SELECT category_id FROM service_category WHERE type = @type",
            ("type", category));

    private sealed class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "SearchTest";

        public TestAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("X-Test-Requester-Id", out var userId)
                || !long.TryParse(userId, out var parsedUserId)
                || parsedUserId <= 0)
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, parsedUserId.ToString()),
                new Claim("role", Request.Headers["X-Test-Role"].ToString())
            };
            var identity = new ClaimsIdentity(claims, SchemeName, ClaimTypes.NameIdentifier, "role");
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}