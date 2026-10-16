using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CivicConnect.Requests;

public sealed class OverdueOptions
{
    public const string SectionName = "Overdue";

    public bool Enabled { get; set; } = true;

    [Range(5, 3600)]
    public int IntervalSeconds { get; set; } = 60;
}

public static class RequestsModuleExtensions
{
    public static IServiceCollection AddRequestsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<OverdueOptions>()
            .BindConfiguration(OverdueOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<RequestStatusTransitionValidator>();
        services.AddSingleton<RequestStatusTransitionValidator>();
        services.AddScoped<OverdueProcessor>();

        if (configuration.GetValue($"{OverdueOptions.SectionName}:Enabled", true))
        {
            services.AddHostedService<OverdueBackgroundService>();
        }

        return services;
    }
}