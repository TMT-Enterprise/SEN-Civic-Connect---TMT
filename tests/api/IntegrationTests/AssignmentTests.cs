using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

using CivicConnect.Data;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Npgsql;

using Xunit;

namespace CivicConnect.IntegrationTests;

[Collection(ApiCollection.Name)]
public class AssignmentTests : IDisposable
{
    // Fixed "now" for the API under test, so the 24-hour boundary is exact.
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    private const string AdminEmail = "thandi.mokoena@example.com";
    private const string StaffEmail = "pieter.vanwyk@example.com";
    private const string OtherStaffEmail = "lerato.dlamini@example.com";

    private readonly ApiFactory _api;
    private readonly WebApplicationFactory<Program> _factory;

    public AssignmentTests(ApiFactory api)
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

                services.RemoveAll<TimeProvider>();
                services.AddSingleton<TimeProvider>(new FixedTimeProvider(Now));
            });
        });
    }

    public void Dispose() => _factory.Dispose();

    // ---------- FR-008: assignment ----------

    [Fact(DisplayName = "FR-008: admin can list active staff members for request assignment")]
    public async Task GetAssignableEmployees_Admin_ReturnsActiveStaffOnly()
    {
        var adminId = await EmployeeIdAsync(AdminEmail);
        var staffId = await EmployeeIdAsync(StaffEmail);
        using var admin = ClientFor(adminId, Roles.Admin);

        var response = await admin.GetAsync("/admin/assignable-employees");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var employees = document.RootElement.EnumerateArray().ToList();
        Assert.Contains(
            employees,
            employee => employee.GetProperty("employeeId").GetInt64() == staffId);
        Assert.DoesNotContain(
            employees,
            employee => employee.GetProperty("employeeId").GetInt64() == adminId);
    }

    [Fact(DisplayName = "NF-002: only admins can list assignable staff")]
    public async Task GetAssignableEmployees_NonAdmin_Returns403()
    {
        var staffId = await EmployeeIdAsync(StaffEmail);
        using var staff = ClientFor(staffId, Roles.Staff);

        var response = await staff.GetAsync("/admin/assignable-employees");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory(DisplayName = "FR-008: due date under 24h is rejected, exactly 24h is accepted")]
    [InlineData(-1L, HttpStatusCode.UnprocessableEntity)]
    [InlineData(-TimeSpan.TicksPerSecond, HttpStatusCode.UnprocessableEntity)]
    [InlineData(0L, HttpStatusCode.OK)]
    [InlineData(TimeSpan.TicksPerSecond, HttpStatusCode.OK)]
    public async Task Assign_DueDateBoundary(long offsetTicks, HttpStatusCode expected)
    {
        var requestId = await SubmitRequestAsync();
        var staffId = await EmployeeIdAsync(StaffEmail);

        var response = await AssignAsync(requestId, staffId, Now.AddHours(24).AddTicks(offsetTicks));

        Assert.Equal(expected, response.StatusCode);
        Assert.Equal(
            expected == HttpStatusCode.OK ? "Assigned" : "Submitted",
            await StatusOfAsync(requestId));
    }

    [Fact(DisplayName = "FR-008: assignment, status update and history row are committed together")]
    public async Task Assign_PersistsAssignmentStatusAndHistoryTogether()
    {
        var requestId = await SubmitRequestAsync();
        var staffId = await EmployeeIdAsync(StaffEmail);

        var response = await AssignAsync(requestId, staffId);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Assigned", await StatusOfAsync(requestId));
        Assert.Equal(
            staffId,
            await _api.ScalarAsync<long>(
                "SELECT COALESCE(assigned_employee_id, 0) FROM service_request WHERE request_id = @id",
                ("id", requestId)));
        Assert.Equal(
            1L,
            await _api.ScalarAsync<long>(
                "SELECT COUNT(*) FROM request_assignment WHERE request_id = @id AND decision = 'Pending'",
                ("id", requestId)));
        Assert.Equal(1L, await HistoryCountAsync(requestId, "Assigned"));
    }

    [Fact(DisplayName = "FR-008: a failing write rolls back assignment, status and history")]
    public async Task Assign_WhenWriteFails_RollsBackEverything()
    {
        var requestId = await SubmitRequestAsync();
        var staffId = await EmployeeIdAsync(StaffEmail);

        // Admin id that does not exist: the assignment/history foreign keys fail inside the transaction.
        using var ghostAdmin = ClientFor(999_999, Roles.Admin);
        var response = await ghostAdmin.PostAsJsonAsync(
            $"/requests/{requestId}/assign",
            new { employeeId = staffId, dueAt = Now.AddDays(2) });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("Submitted", await StatusOfAsync(requestId));
        Assert.Equal(
            0L,
            await _api.ScalarAsync<long>(
                "SELECT COALESCE(assigned_employee_id, 0) FROM service_request WHERE request_id = @id",
                ("id", requestId)));
        Assert.Equal(
            0L,
            await _api.ScalarAsync<long>(
                "SELECT COUNT(*) FROM request_assignment WHERE request_id = @id",
                ("id", requestId)));
        Assert.Equal(0L, await HistoryCountAsync(requestId, "Assigned"));
    }

    [Fact(DisplayName = "FR-008: concurrent assignment returns 409 to every caller but one")]
    public async Task Assign_ConcurrentCallers_ExactlyOneSucceeds()
    {
        var requestId = await SubmitRequestAsync();
        var employees = new[] { await EmployeeIdAsync(StaffEmail), await EmployeeIdAsync(OtherStaffEmail) };

        var calls = Enumerable.Range(0, 5)
            .Select(i => AssignAsync(requestId, employees[i % 2]))
            .ToArray();
        var responses = await Task.WhenAll(calls);
        var codes = responses.Select(r => r.StatusCode).ToList();

        Assert.Single(codes, HttpStatusCode.OK);
        Assert.Equal(4, codes.Count(c => c == HttpStatusCode.Conflict));

        Assert.Equal(
            1L,
            await _api.ScalarAsync<long>(
                "SELECT COUNT(*) FROM request_assignment WHERE request_id = @id AND decision IN ('Pending', 'Accepted')",
                ("id", requestId)));
        Assert.Equal(
            1L,
            await _api.ScalarAsync<long>(
                """
                SELECT COUNT(*) FROM request_assignment a
                JOIN service_request r ON r.request_id = a.request_id AND r.assigned_employee_id = a.employee_id
                WHERE a.request_id = @id AND a.decision = 'Pending'
                """,
                ("id", requestId)));
        Assert.Equal(1L, await HistoryCountAsync(requestId, "Assigned"));
    }

    [Fact(DisplayName = "FR-008: assigning an already assigned request returns 409")]
    public async Task Assign_AlreadyAssigned_Returns409()
    {
        var requestId = await SubmitRequestAsync();
        var staffId = await EmployeeIdAsync(StaffEmail);

        Assert.Equal(HttpStatusCode.OK, (await AssignAsync(requestId, staffId)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await AssignAsync(requestId, staffId)).StatusCode);
    }

    [Theory(DisplayName = "NF-002: only management can assign")]
    [InlineData(Roles.Staff)]
    [InlineData(Roles.Requester)]
    public async Task Assign_ByNonAdminRole_Returns403(string role)
    {
        var requestId = await SubmitRequestAsync();
        var staffId = await EmployeeIdAsync(StaffEmail);

        using var client = ClientFor(staffId, role);
        var response = await client.PostAsJsonAsync(
            $"/requests/{requestId}/assign",
            new { employeeId = staffId, dueAt = Now.AddDays(2) });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("Submitted", await StatusOfAsync(requestId));
    }

    [Theory(DisplayName = "NF-002: decide and status are staff only")]
    [InlineData(Roles.Admin)]
    [InlineData(Roles.Requester)]
    public async Task DecideAndStatus_ByNonStaffRole_Return403(string role)
    {
        var requestId = await SubmitRequestAsync();
        var staffId = await EmployeeIdAsync(StaffEmail);
        await AssignAsync(requestId, staffId);

        var decide = await PostAsAsync(staffId, role, $"/requests/{requestId}/decide", new { decision = "Accepted" });
        var status = await PatchAsAsync(staffId, role, $"/requests/{requestId}/status", new { status = "InProgress" });

        Assert.Equal(HttpStatusCode.Forbidden, decide.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, status.StatusCode);
        Assert.Equal("Assigned", await StatusOfAsync(requestId));
    }

    [Fact(DisplayName = "NF-002: endpoints reject unauthenticated callers")]
    public async Task EmployeeEndpoints_WithoutAuthentication_Return401()
    {
        using var anonymous = _factory.CreateClient();

        foreach (var url in new[] { "/requests/1/assign", "/requests/1/decide" })
        {
            var response = await anonymous.PostAsJsonAsync(url, new { });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await anonymous.GetAsync("/admin/assignable-employees")).StatusCode);
        var patch = await anonymous.PatchAsync("/requests/1/status", JsonContent.Create(new { }));
        Assert.Equal(HttpStatusCode.Unauthorized, patch.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/requests/1/history")).StatusCode);
    }

    // ---------- FR-010: accept ----------

    [Fact(DisplayName = "FR-010: assignee accepts; a second accept or a later reject returns 409")]
    public async Task Decide_Accept_MarksAcceptedAndCannotBeRepeatedOrRejected()
    {
        var requestId = await SubmitRequestAsync();
        var staffId = await EmployeeIdAsync(StaffEmail);
        await AssignAsync(requestId, staffId);

        var accept = await DecideAsync(staffId, requestId, new { decision = "Accepted" });

        Assert.Equal(HttpStatusCode.OK, accept.StatusCode);
        Assert.Equal(
            1L,
            await _api.ScalarAsync<long>(
                "SELECT COUNT(*) FROM request_assignment WHERE request_id = @id AND decision = 'Accepted' AND decision_at IS NOT NULL",
                ("id", requestId)));
        Assert.Equal("Assigned", await StatusOfAsync(requestId));

        var again = await DecideAsync(staffId, requestId, new { decision = "Accepted" });
        var reject = await DecideAsync(staffId, requestId, new { decision = "Rejected", rejectionComment = "Too late" });

        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, reject.StatusCode);
    }

    [Fact(DisplayName = "FR-010: an unknown decision value returns 422")]
    public async Task Decide_WithUnknownDecision_Returns422()
    {
        var requestId = await SubmitRequestAsync();
        var staffId = await EmployeeIdAsync(StaffEmail);
        await AssignAsync(requestId, staffId);

        var response = await DecideAsync(staffId, requestId, new { decision = "Maybe" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal(
            0L,
            await _api.ScalarAsync<long>(
                "SELECT COUNT(*) FROM request_assignment WHERE request_id = @id AND decision <> 'Pending'",
                ("id", requestId)));
    }

    // ---------- FR-011: reject ----------

    [Fact(DisplayName = "FR-011: rejection stores the comment and returns the request to the unassigned pool")]
    public async Task Decide_Reject_StoresCommentAndReturnsRequestToPool()
    {
        var requestId = await SubmitRequestAsync();
        var staffId = await EmployeeIdAsync(StaffEmail);
        var otherStaffId = await EmployeeIdAsync(OtherStaffEmail);
        await AssignAsync(requestId, staffId);

        var response = await DecideAsync(
            staffId,
            requestId,
            new { decision = "Rejected", rejectionComment = "On leave that week" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Submitted", await StatusOfAsync(requestId));
        Assert.Equal(
            "On leave that week",
            await _api.ScalarAsync<string>(
                "SELECT rejection_comment FROM request_assignment WHERE request_id = @id AND decision = 'Rejected'",
                ("id", requestId)));
        Assert.Equal(
            0L,
            await _api.ScalarAsync<long>(
                "SELECT COALESCE(assigned_employee_id, 0) FROM service_request WHERE request_id = @id",
                ("id", requestId)));
        Assert.Equal(1L, await HistoryCountAsync(requestId, "Assigned"));
        Assert.Equal(1L, await HistoryCountAsync(requestId, "Submitted"));

        // Back in the pool: it can be assigned again, and the rejected row is kept for audit.
        var reassign = await AssignAsync(requestId, otherStaffId);
        Assert.Equal(HttpStatusCode.OK, reassign.StatusCode);
        Assert.Equal(
            1L,
            await _api.ScalarAsync<long>(
                "SELECT COUNT(*) FROM request_assignment WHERE request_id = @id AND decision = 'Rejected'",
                ("id", requestId)));
    }

    [Theory(DisplayName = "FR-011: rejection without a comment returns 422 and changes nothing")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Decide_RejectWithoutComment_Returns422(string? comment)
    {
        var requestId = await SubmitRequestAsync();
        var staffId = await EmployeeIdAsync(StaffEmail);
        await AssignAsync(requestId, staffId);

        var response = await DecideAsync(
            staffId,
            requestId,
            new { decision = "Rejected", rejectionComment = comment });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("Assigned", await StatusOfAsync(requestId));
    }

    [Fact(DisplayName = "FR-010/FR-011: another employee cannot decide on someone else's assignment")]
    public async Task Decide_ByOtherEmployee_Returns403()
    {
        var requestId = await SubmitRequestAsync();
        var staffId = await EmployeeIdAsync(StaffEmail);
        var otherStaffId = await EmployeeIdAsync(OtherStaffEmail);
        await AssignAsync(requestId, staffId);

        var accept = await DecideAsync(otherStaffId, requestId, new { decision = "Accepted" });
        var reject = await DecideAsync(
            otherStaffId,
            requestId,
            new { decision = "Rejected", rejectionComment = "Not mine" });

        Assert.Equal(HttpStatusCode.Forbidden, accept.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, reject.StatusCode);
        Assert.Equal("Assigned", await StatusOfAsync(requestId));
    }

    // ---------- FR-013: status changes ----------

    [Fact(DisplayName = "FR-013: assignee moves a request to In Progress and closes it with a completion note")]
    public async Task StatusChange_FullLifecycle_WritesHistoryAtEachStep()
    {
        var requestId = await SubmitRequestAsync();
        var staffId = await EmployeeIdAsync(StaffEmail);
        await AssignAsync(requestId, staffId);
        await DecideAsync(staffId, requestId, new { decision = "Accepted" });

        var inProgress = await UpdateStatusAsync(staffId, requestId, new { status = "InProgress" });
        Assert.Equal(HttpStatusCode.OK, inProgress.StatusCode);
        Assert.Equal("In Progress", await StatusOfAsync(requestId));

        var closeWithoutNote = await UpdateStatusAsync(staffId, requestId, new { status = "Closed" });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, closeWithoutNote.StatusCode);
        Assert.Equal("In Progress", await StatusOfAsync(requestId));

        var closed = await UpdateStatusAsync(
            staffId,
            requestId,
            new { status = "Closed", completionNote = "Repaired and verified." });
        Assert.Equal(HttpStatusCode.OK, closed.StatusCode);
        Assert.Equal("Closed", await StatusOfAsync(requestId));
        Assert.Equal(
            1L,
            await _api.ScalarAsync<long>(
                "SELECT COUNT(*) FROM service_request WHERE request_id = @id AND closed_at IS NOT NULL",
                ("id", requestId)));

        Assert.Equal(1L, await HistoryCountAsync(requestId, "Assigned"));
        Assert.Equal(1L, await HistoryCountAsync(requestId, "In Progress"));
        Assert.Equal(1L, await HistoryCountAsync(requestId, "Closed"));
    }

    [Fact(DisplayName = "FR-013: a disallowed transition returns 409 and changes nothing")]
    public async Task StatusChange_InvalidTransition_Returns409()
    {
        var requestId = await SubmitRequestAsync();
        var staffId = await EmployeeIdAsync(StaffEmail);
        await AssignAsync(requestId, staffId);

        // Assigned -> Closed skips InProgress.
        var response = await UpdateStatusAsync(
            staffId,
            requestId,
            new { status = "Closed", completionNote = "Skipped ahead" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("Assigned", await StatusOfAsync(requestId));
        Assert.Equal(0L, await HistoryCountAsync(requestId, "Closed"));
    }

    [Fact(DisplayName = "FR-013: staff who are not assigned to the request cannot change its status")]
    public async Task StatusChange_ByUnassignedStaff_Returns403()
    {
        var requestId = await SubmitRequestAsync();
        var staffId = await EmployeeIdAsync(StaffEmail);
        var otherStaffId = await EmployeeIdAsync(OtherStaffEmail);
        await AssignAsync(requestId, staffId);

        var response = await UpdateStatusAsync(otherStaffId, requestId, new { status = "InProgress" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("Assigned", await StatusOfAsync(requestId));
    }

    [Theory(DisplayName = "FR-013: Assigned, Submitted and unknown values are not valid status targets")]
    [InlineData("Assigned")]
    [InlineData("Submitted")]
    [InlineData("Bogus")]
    public async Task StatusChange_ToUnsupportedTarget_Returns422(string target)
    {
        var requestId = await SubmitRequestAsync();
        var staffId = await EmployeeIdAsync(StaffEmail);
        await AssignAsync(requestId, staffId);

        var response = await UpdateStatusAsync(staffId, requestId, new { status = target });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("Assigned", await StatusOfAsync(requestId));
    }

    // ---------- history is insert-and-read only ----------

    [Fact(DisplayName = "FR-013: history is readable by employees only")]
    public async Task History_IsReadableByEmployees()
    {
        var requestId = await SubmitRequestAsync();
        var staffId = await EmployeeIdAsync(StaffEmail);
        await AssignAsync(requestId, staffId);

        using var staff = ClientFor(staffId, Roles.Staff);
        var response = await staff.GetAsync($"/requests/{requestId}/history");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var entries = doc.RootElement.EnumerateArray().ToList();
        Assert.Single(entries);
        Assert.Equal("Assigned", entries[0].GetProperty("status").GetString());

        using var requester = ClientFor(1, Roles.Requester);
        Assert.Equal(HttpStatusCode.Forbidden, (await requester.GetAsync($"/requests/{requestId}/history")).StatusCode);
    }

    [Fact(DisplayName = "FR-013: the database refuses to update or delete history rows")]
    public async Task History_CannotBeUpdatedOrDeleted_InDatabase()
    {
        var requestId = await SubmitRequestAsync();
        await AssignAsync(requestId, await EmployeeIdAsync(StaffEmail));

        await Assert.ThrowsAsync<PostgresException>(() => _api.ExecuteAsync(
            "UPDATE request_status_history SET changed_at = now() WHERE request_id = @id",
            ("id", requestId)));
        await Assert.ThrowsAsync<PostgresException>(() => _api.ExecuteAsync(
            "DELETE FROM request_status_history WHERE request_id = @id",
            ("id", requestId)));

        Assert.Equal(1L, await HistoryCountAsync(requestId, "Assigned"));
    }

    [Fact(DisplayName = "FR-013: EF refuses to modify or delete history rows")]
    public async Task History_CannotBeModified_ThroughEf()
    {
        var requestId = await SubmitRequestAsync();
        await AssignAsync(requestId, await EmployeeIdAsync(StaffEmail));

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var row = await db.RequestStatusHistory.FirstAsync(h => h.RequestId == requestId);
            row.ChangedAt = row.ChangedAt.AddMinutes(1);

            await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var row = await db.RequestStatusHistory.FirstAsync(h => h.RequestId == requestId);
            db.RequestStatusHistory.Remove(row);

            await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
        }
    }

    // ---------- helpers ----------

    private HttpClient ClientFor(long userId, string role)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Requester-Id", userId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Role", role);
        return client;
    }

    private async Task<HttpResponseMessage> PostAsAsync(long userId, string role, string url, object body)
    {
        using var client = ClientFor(userId, role);
        return await client.PostAsJsonAsync(url, body);
    }

    private async Task<HttpResponseMessage> PatchAsAsync(long userId, string role, string url, object body)
    {
        using var client = ClientFor(userId, role);
        return await client.PatchAsync(url, JsonContent.Create(body));
    }

    private Task<HttpResponseMessage> DecideAsync(long staffId, long requestId, object body) =>
        PostAsAsync(staffId, Roles.Staff, $"/requests/{requestId}/decide", body);

    private Task<HttpResponseMessage> UpdateStatusAsync(long staffId, long requestId, object body) =>
        PatchAsAsync(staffId, Roles.Staff, $"/requests/{requestId}/status", body);

    private async Task<HttpResponseMessage> AssignAsync(long requestId, long employeeId, DateTimeOffset? dueAt = null)
    {
        var adminId = await EmployeeIdAsync(AdminEmail);
        using var admin = ClientFor(adminId, Roles.Admin);
        return await admin.PostAsJsonAsync(
            $"/requests/{requestId}/assign",
            new { employeeId, dueAt = dueAt ?? Now.AddDays(2) });
    }

    private async Task<long> SubmitRequestAsync()
    {
        using var requester = ClientFor(1, Roles.Requester);
        var response = await requester.PostAsJsonAsync(
            "/requests",
            new { categoryId = 1, description = $"S2-07 test {Guid.NewGuid()}" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("requestId").GetInt64();
    }

    private Task<long> EmployeeIdAsync(string email) =>
        _api.ScalarAsync<long>("SELECT employee_id FROM employee WHERE email = @email", ("email", email));

    private Task<string?> StatusOfAsync(long requestId) =>
        _api.ScalarAsync<string>(
            """
            SELECT s.name FROM service_request r
            JOIN request_status s ON s.status_id = r.current_status_id
            WHERE r.request_id = @id
            """,
            ("id", requestId));

    private Task<long> HistoryCountAsync(long requestId, string status) =>
        _api.ScalarAsync<long>(
            """
            SELECT COUNT(*) FROM request_status_history h
            JOIN request_status s ON s.status_id = h.status_id
            WHERE h.request_id = @id AND s.name = @status
            """,
            ("id", requestId),
            ("status", status));

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
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