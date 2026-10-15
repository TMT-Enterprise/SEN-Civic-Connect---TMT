using CivicConnect.Auth;
using CivicConnect.Data;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CivicConnect.Api;

public static class ApiAuthExtensions
{
    public static IServiceCollection AddApiAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

        // Configured from JwtOptions lazily, so the secret is read from the same validated options the token service uses.
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;

                bearer.MapInboundClaims = false;
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = jwt.CreateSigningKey(),
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = "sub",
                    RoleClaimType = "role"
                };

                // S2-04: replace the default empty 401/403 with the same ProblemDetails shape the rest of the
                // API returns (PED §9.3), without leaking why validation failed.
                bearer.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.Headers.WWWAuthenticate = JwtBearerDefaults.AuthenticationScheme;
                        return Results.Problem(
                                title: "Authentication required",
                                statusCode: StatusCodes.Status401Unauthorized)
                            .ExecuteAsync(context.HttpContext);
                    },
                    OnForbidden = context => Results.Problem(
                            title: "Role not permitted",
                            statusCode: StatusCodes.Status403Forbidden)
                        .ExecuteAsync(context.HttpContext)
                };
            });

        services.AddAuthorizationBuilder()
            // S2-04 / ASR-1: deny by default. An endpoint with no authorization metadata still requires a
            // valid token, so a route nobody remembered to protect cannot be public by accident.
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())
            .AddPolicy(Policies.RequesterOnly, policy => policy.RequireRole(Roles.Requester))
            .AddPolicy(Policies.StaffOnly, policy => policy.RequireRole(Roles.Staff))
            .AddPolicy(Policies.AdminOnly, policy => policy.RequireRole(Roles.Admin))
            .AddPolicy(Policies.AnyEmployee, policy => policy.RequireRole(Roles.Employee));

        return services;
    }
}