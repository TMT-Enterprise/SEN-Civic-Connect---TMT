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
            SubmitRequestCommand command,
            RequestsService service,
            CancellationToken cancellationToken) =>
        {
            // TODO: replace with JWT claims once auth is implemented
            const long placeholderRequesterId = 1;

            try
            {
                var result = await service.SubmitAsync(
                    placeholderRequesterId,
                    command,
                    cancellationToken);

                return Results.Created(
                    $"/requests/{result.RequestId}",
                    result);
            }
            catch (ArgumentException ex)
            {
                return Results.UnprocessableEntity(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Problem(
                    title: "Submission failed",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        });

        // GET /requests — list requester's own requests
        endpoints.MapGet("/requests", async (
            RequestsService service,
            CancellationToken cancellationToken) =>
        {
            // TODO: replace with JWT claims once auth is implemented
            const long placeholderRequesterId = 1;

            var requests = await service.GetRequesterHistoryAsync(
                placeholderRequesterId,
                cancellationToken);

            return Results.Ok(requests);
        });

        // GET /requests/{id} — get one request
        endpoints.MapGet("/requests/{id:long}", async (
            long id,
            RequestsService service,
            CancellationToken cancellationToken) =>
        {
            // TODO: replace with JWT claims once auth is implemented
            const long placeholderRequesterId = 1;

            var request = await service.GetByIdAsync(
                id,
                placeholderRequesterId,
                cancellationToken);

            return request is null
                ? Results.NotFound()
                : Results.Ok(request);
        });

        return endpoints;
    }
}