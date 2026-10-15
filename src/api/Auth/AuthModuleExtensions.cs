using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CivicConnect.Auth;

public static class AuthModuleExtensions
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services)
    {
        // Fails at startup, not at first login, if the secret/issuer/audience/expiry are missing or invalid.
        services.AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddScoped<AuthService>();

        return services;
    }
}