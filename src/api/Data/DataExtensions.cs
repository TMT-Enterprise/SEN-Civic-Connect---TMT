using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CivicConnect.Data;

public static class DataExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<AppendOnlyHistoryInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Default"));
            options.AddInterceptors(sp.GetRequiredService<AppendOnlyHistoryInterceptor>());
        });

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IOverdueRepository, OverdueRepository>();

        return services;
    }
}