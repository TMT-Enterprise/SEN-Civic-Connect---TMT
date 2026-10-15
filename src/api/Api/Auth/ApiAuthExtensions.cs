using CivicConnect.Auth;
using CivicConnect.Data;

using Microsoft.AspNetCore.Authentication.JwtBearer;
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
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.RequesterOnly, policy => policy.RequireRole(Roles.Requester))
            .AddPolicy(Policies.StaffOnly, policy => policy.RequireRole(Roles.Staff))
            .AddPolicy(Policies.AdminOnly, policy => policy.RequireRole(Roles.Admin))
            .AddPolicy(Policies.AnyEmployee, policy => policy.RequireRole(Roles.Employee));

        return services;
    }
}