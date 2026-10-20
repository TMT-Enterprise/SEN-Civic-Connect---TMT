using System.Security.Claims;

using CivicConnect.Data;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CivicConnect.Requests;

public static class AssignmentEndpoints
{
    public static IEndpointRouteBuilder MapAssignmentEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/admin/assignable-employees", async (
            AssignmentService service,
            CancellationToken ct) =>
            Results.Ok(await service.GetAssignableEmployeesAsync(ct)))
            .RequireAuthorization(policy => policy.RequireRole(Roles.Admin));

        // POST /requests/{id}/assign (FR-008): management only
        endpoints.MapPost("/requests/{id:long}/assign", async (
            HttpContext context,
            long id,
            AssignRequestDto dto,
            AssignmentService service,
            CancellationToken ct) =>
        {
            if (!TryGetEmployeeId(context.User, out var actorId))
            {
                return Results.Unauthorized();
            }

            return ToHttp(await service.AssignAsync(id, dto.EmployeeId, dto.DueAt, actorId, ct));
        }).RequireAuthorization(policy => policy.RequireRole(Roles.Admin));

        // POST /requests/{id}/decide (FR-010, FR-011): assigned staff only
        endpoints.MapPost("/requests/{id:long}/decide", async (
            HttpContext context,
            long id,
            DecideRequestDto dto,
            AssignmentService service,
            CancellationToken ct) =>
        {
            if (!TryGetEmployeeId(context.User, out var actorId))
            {
                return Results.Unauthorized();
            }

            if (string.Equals(dto.Decision, "Accepted", StringComparison.OrdinalIgnoreCase))
            {
                return ToHttp(await service.AcceptAsync(id, actorId, ct));
            }

            if (string.Equals(dto.Decision, "Rejected", StringComparison.OrdinalIgnoreCase))
            {
                return ToHttp(await service.RejectAsync(id, actorId, dto.RejectionComment, ct));
            }

            return Results.UnprocessableEntity(new { error = "Decision must be Accepted or Rejected." });
        }).RequireAuthorization(policy => policy.RequireRole(Roles.Staff));

        // PATCH /requests/{id}/status (FR-011, FR-013): assigned staff only
        endpoints.MapPatch("/requests/{id:long}/status", async (
            HttpContext context,
            long id,
            UpdateStatusDto dto,
            AssignmentService service,
            CancellationToken ct) =>
        {
            if (!TryGetEmployeeId(context.User, out var actorId))
            {
                return Results.Unauthorized();
            }

            if (!TryParseTarget(dto.Status, out var target))
            {
                return Results.UnprocessableEntity(new { error = "Status must be InProgress, Resolved or Closed." });
            }

            var result = await service.ChangeStatusAsync(
                id,
                target,
                dto.CompletionNote,
                actorId,
                context.User.IsInRole(Roles.Admin),
                ct);

            return ToHttp(result);
        }).RequireAuthorization(policy => policy.RequireRole(Roles.Staff));

        // GET /requests/{id}/history: read-only. There is deliberately no PUT/PATCH/DELETE.
        endpoints.MapGet("/requests/{id:long}/history", async (
            long id,
            AssignmentService service,
            CancellationToken ct) =>
            Results.Ok(await service.GetHistoryAsync(id, ct)))
            .RequireAuthorization(policy => policy.RequireRole(Roles.Admin, Roles.Staff));

        return endpoints;
    }

    // Assigned and Submitted are only ever set through assign and reject.
    private static bool TryParseTarget(string? value, out RequestStatus status) =>
        Enum.TryParse(value, ignoreCase: true, out status)
        && Enum.IsDefined(status)
        && status is not (RequestStatus.Submitted or RequestStatus.Assigned);

    private static IResult ToHttp<T>(Result<T> result) =>
        result.Outcome switch
        {
            Outcome.Ok => Results.Ok(result.Value),
            Outcome.NotFound => Results.NotFound(new { error = result.Error }),
            Outcome.Conflict => Results.Conflict(new { error = result.Error }),
            Outcome.Forbidden => Results.Json(new { error = result.Error }, statusCode: StatusCodes.Status403Forbidden),
            _ => Results.UnprocessableEntity(new { error = result.Error })
        };

    private static bool TryGetEmployeeId(ClaimsPrincipal user, out long employeeId)
    {
        employeeId = 0;

        if (user.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var subject = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub");

        return long.TryParse(subject, out employeeId) && employeeId > 0;
    }
}