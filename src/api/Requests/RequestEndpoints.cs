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
        // POST /requests — submit a new service request
        endpoints.MapPost("/requests", async (
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
                    $"/requests/{result.RequestId}",
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
        endpoints.MapGet("/requests", async (
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

        // GET /requests/{id} — get one request
        endpoints.MapGet("/requests/{id:long}", async (
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

        return endpoints;
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