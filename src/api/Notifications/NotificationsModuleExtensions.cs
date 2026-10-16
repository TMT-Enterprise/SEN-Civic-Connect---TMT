using CivicConnect.Requests;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CivicConnect.Notifications;

public static class NotificationsModuleExtensions
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services)
    {
        // Fails at startup, not at the first status change, if email is switched on but incomplete.
        services.AddOptions<EmailOptions>()
            .BindConfiguration(EmailOptions.SectionName)
            .ValidateDataAnnotations()
            .Validate(
                o => !o.Enabled || (!string.IsNullOrWhiteSpace(o.Host) && !string.IsNullOrWhiteSpace(o.FromAddress)),
                "Email:Host and Email:FromAddress are required when Email:Enabled is true.")
            .ValidateOnStart();

        services.AddSingleton<INotifier>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<EmailOptions>>();
            if (options.Value.Enabled)
            {
                return new EmailNotifier(options);
            }

            return new DisabledNotifier();
        });

        return services;
    }
}