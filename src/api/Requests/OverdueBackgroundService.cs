using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CivicConnect.Requests;

public sealed class OverdueBackgroundService(
    IServiceScopeFactory scopes,
    IOptions<OverdueOptions> options,
    ILogger<OverdueBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield(); // never hold up host start-up

        try
        {
            // First run is immediate: it flags everything that fell due while the host was stopped or asleep.
            await RunSafelyAsync(stoppingToken);

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(options.Value.IntervalSeconds));
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RunSafelyAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // normal shutdown
        }
    }

    private async Task RunSafelyAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopes.CreateAsyncScope();
            var processor = scope.ServiceProvider.GetRequiredService<OverdueProcessor>();
            await processor.RunOnceAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // A missing or unreachable database must not stop the API; the next tick tries again. in theory that is :)
            logger.LogWarning(ex, "Overdue run failed; will retry on the next interval.");
        }
    }
}