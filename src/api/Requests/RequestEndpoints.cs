using System.Security.Claims;

using CivicConnect.Data;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CivicConnect.Requests.Requests;

public static class RequestsEndpoints
{
    public static IEndpointRouteBuilder MapRequestsEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var api = endpoints.MapGroup("/api/v1");
        // POST /requests — submit a new service request
        api.MapPost("/requests", async (
            HttpContext context,
            SubmitRequestCommand command,
            RequestsService service,
            CancellationToken cancellationToken) =>
        {
            if (!TryGetRequesterId(context.User, out var requesterId))
            {
                return Results.Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(command.Description))
            {
                return Results.ValidationProblem(
                    new Dictionary<string, string[]>
                    {
                        ["description"] = ["Description is required."]
                    },
                    statusCode: StatusCodes.Status422UnprocessableEntity);
            }

            try
            {
                var result = await service.SubmitAsync(
                    requesterId,
                    command,
                    cancellationToken);

                return Results.Created(
                    $"/api/v1/requests/{result.RequestId}",
                    result);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(
                    new Dictionary<string, string[]>
                    {
                        ["categoryId"] = [ex.Message]
                    },
                    statusCode: StatusCodes.Status422UnprocessableEntity);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Problem(
                    title: "Submission failed",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }).RequireAuthorization(policy => policy.RequireRole(Roles.Requester));

        // GET /requests — list requester's own requests
        api.MapGet("/requests", async (
            HttpContext context,
            RequestsService service,
            CancellationToken cancellationToken) =>
        {
            if (!TryGetRequesterId(context.User, out var requesterId))
            {
                return Results.Unauthorized();
            }

            var requests = await service.GetRequesterHistoryAsync(
                requesterId,
                cancellationToken);

            return Results.Ok(requests);
        }).RequireAuthorization(policy => policy.RequireRole(Roles.Requester));

        api.MapGet("/staff/requests", async (
            HttpContext context,
            string? search,
            string? id,
            string? status,
            string? dueDate,
            string? category,
            string? assignee,
            long? categoryId,
            RequestsService service,
            CancellationToken cancellationToken) =>
        {
            if (!TryGetEmployeeId(context.User, out var employeeId))
            {
                return Results.Unauthorized();
            }

            if (!TryCreateFilters(
                    search,
                    id,
                    status,
                    dueDate,
                    category,
                    assignee,
                    categoryId,
                    assignedEmployeeId: null,
                    unassigned: false,
                    out var filters))
            {
                return Results.ValidationProblem(
                    new Dictionary<string, string[]>
                    {
                        ["dueDate"] = ["Use an ISO date or date prefix (yyyy, yyyy-MM, or yyyy-MM-dd)."]
                    });
            }

            return Results.Ok(await service.SearchStaffAsync(
                employeeId,
                filters,
                cancellationToken));
        })
        .RequireAuthorization(policy => policy.RequireRole(Roles.Staff))
        .WithName("SearchStaffRequests")
        .WithSummary("Search and filter requests assigned to the authenticated staff member")
        .WithDescription("Returns only requests assigned to the authenticated staff member. Text filters are case-insensitive partial matches.")
        .Produces<RequestSearchResponse>()
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);

        api.MapGet("/admin/requests", async (
            HttpContext context,
            string? search,
            string? id,
            string? status,
            string? dueDate,
            string? category,
            string? assignee,
            long? categoryId,
            long? assignedEmployeeId,
            bool? unassigned,
            RequestsService service,
            CancellationToken cancellationToken) =>
        {
            if (!TryGetEmployeeId(context.User, out _))
            {
                return Results.Unauthorized();
            }

            if (!TryCreateFilters(
                    search,
                    id,
                    status,
                    dueDate,
                    category,
                    assignee,
                    categoryId,
                    assignedEmployeeId,
                    unassigned ?? false,
                    out var filters))
            {
                return Results.ValidationProblem(
                    new Dictionary<string, string[]>
                    {
                        ["dueDate"] = ["Use an ISO date or date prefix (yyyy, yyyy-MM, or yyyy-MM-dd)."]
                    });
            }

            return Results.Ok(await service.SearchAdminAsync(filters, cancellationToken));
        })
        .RequireAuthorization(policy => policy.RequireRole(Roles.Admin))
        .WithName("SearchAdminRequests")
        .WithSummary("Search and filter all requests, including unassigned requests")
        .WithDescription("Admin-only request queue. Text filters are case-insensitive partial matches.")
        .Produces<RequestSearchResponse>()
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);

        // GET /requests/{id} — get one request
        api.MapGet("/requests/{id:long}", async (
            HttpContext context,
            long id,
            RequestsService service,
            CancellationToken cancellationToken) =>
        {
            if (!TryGetRequesterId(context.User, out var requesterId))
            {
                return Results.Unauthorized();
            }

            var request = await service.GetByIdAsync(
                id,
                requesterId,
                cancellationToken);

            return request is null
                ? Results.NotFound()
                : Results.Ok(request);
        }).RequireAuthorization(policy => policy.RequireRole(Roles.Requester));

        api.MapGet("/management/requests", async (
            string? search, string? id, string? status, string? dueDate, string? category, string? assignee,
            long? categoryId, long? assignedEmployeeId, bool? unassigned, RequestsService service, CancellationToken ct) =>
        {
            if (!TryCreateFilters(search, id, status, dueDate, category, assignee, categoryId, assignedEmployeeId, unassigned ?? false, out var filters))
                return Results.ValidationProblem(new Dictionary<string, string[]> { ["dueDate"] = ["Use an ISO date or date prefix (yyyy, yyyy-MM, or yyyy-MM-dd)."] });
            return Results.Ok(await service.SearchAdminAsync(filters, ct));
        }).RequireAuthorization(policy => policy.RequireRole(Roles.Admin));

        api.MapGet("/staff/requests/{id:long}", async (HttpContext context, long id, RequestsService service, CancellationToken ct) =>
        {
            if (!TryGetEmployeeId(context.User, out var employeeId)) return Results.Unauthorized();
            var request = await service.GetForStaffAsync(id, employeeId, ct);
            return request is null ? Results.NotFound() : Results.Ok(request);
        }).RequireAuthorization(policy => policy.RequireRole(Roles.Staff));

        api.MapGet("/admin/requests/{id:long}", async (long id, RequestsService service, CancellationToken ct) =>
        {
            var request = await service.GetForAdminAsync(id, ct);
            return request is null ? Results.NotFound() : Results.Ok(request);
        }).RequireAuthorization(policy => policy.RequireRole(Roles.Admin));

        return endpoints;
    }

    private static bool TryCreateFilters(
        string? search,
        string? id,
        string? status,
        string? dueDate,
        string? category,
        string? assignee,
        long? categoryId,
        long? assignedEmployeeId,
        bool unassigned,
        out RequestSearchFilters filters)
    {
        RequestSearchDateRange? dateRange = null;
        if (!string.IsNullOrWhiteSpace(dueDate))
        {
            if (!RequestSearchDateRange.TryParse(dueDate, out var parsedDateRange))
            {
                filters = new RequestSearchFilters();
                return false;
            }

            dateRange = parsedDateRange;
        }

        filters = new RequestSearchFilters(
            search,
            id,
            status,
            dateRange,
            category,
            assignee,
            categoryId,
            assignedEmployeeId,
            unassigned);
        return true;
    }

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

    private static bool TryGetRequesterId(ClaimsPrincipal user, out long requesterId)
    {
        requesterId = 0;

        if (user.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var subject = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub");

        return long.TryParse(subject, out requesterId) && requesterId > 0;
    }
}