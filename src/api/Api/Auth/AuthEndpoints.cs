using System.Security.Claims;

using CivicConnect.Auth;

namespace CivicConnect.Api;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth").WithTags("Auth");

        group.MapPost("/register", RegisterAsync).AllowAnonymous();
        group.MapPost("/login", LoginAsync).AllowAnonymous();
        group.MapGet("/me", Me).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        AuthService auth,
        CancellationToken cancellationToken)
    {
        var result = await auth.RegisterAsync(request, cancellationToken);

        return result.Status switch
        {
            RegisterStatus.Created => Results.Json(result.Account, statusCode: StatusCodes.Status201Created),
            RegisterStatus.DuplicateEmail => Results.Problem(
                title: "Email already registered",
                detail: "An account with this email already exists.",
                statusCode: StatusCodes.Status409Conflict),
            _ => Results.ValidationProblem(result.Errors!, statusCode: StatusCodes.Status422UnprocessableEntity)
        };
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        AuthService auth,
        CancellationToken cancellationToken)
    {
        var success = await auth.LoginAsync(request, cancellationToken);

        return success is null
            ? Results.Problem(title: "Invalid email or password", statusCode: StatusCodes.Status401Unauthorized)
            : Results.Ok(success);
    }

    private static IResult Me(ClaimsPrincipal user) => Results.Ok(new
    {
        id = user.FindFirst("sub")?.Value,
        accountType = user.FindFirst("acct")?.Value,
        email = user.FindFirst("email")?.Value,
        role = user.FindFirst("role")?.Value
    });
}