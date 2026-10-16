using System.Diagnostics;
using System.Globalization;

using CivicConnect.Notifications;
using CivicConnect.Requests;

using Microsoft.Extensions.Options;

using Xunit.Abstractions;

namespace CivicConnect.IntegrationTests;

public sealed class LiveEmailFactAttribute : FactAttribute
{
    public LiveEmailFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("CIVICCONNECT_LIVE_EMAIL_TO")))
        {
            Skip = "Set CIVICCONNECT_LIVE_EMAIL_TO and the Email__* variables to run the live send.";
        }
    }
}

public sealed class LiveEmailTimingTests(ITestOutputHelper output)
{
    private static string Env(string name) =>
        Environment.GetEnvironmentVariable(name) ?? throw new InvalidOperationException($"{name} is not set.");

    [LiveEmailFact(DisplayName = "S2-09 / FR-006 / ASR-3 / R-14: one real send is accepted by the provider within the 60 second target")]
    public async Task One_real_send_is_within_the_target()
    {
        var settings = new EmailOptions
        {
            Enabled = true,
            Host = Env("Email__Host"),
            Port = int.Parse(Env("Email__Port"), CultureInfo.InvariantCulture),
            Username = Env("Email__Username"),
            Password = Env("Email__Password"),
            FromAddress = Env("Email__FromAddress"),
            FromName = "CivicConnect"
        };

        var notifier = new EmailNotifier(Options.Create(settings));
        var notification = new StatusChangeNotification(0, 1, "In Progress", "Live Test", Env("CIVICCONNECT_LIVE_EMAIL_TO"));

        var started = Stopwatch.GetTimestamp();
        await notifier.SendAsync(notification, CancellationToken.None);
        var elapsed = Stopwatch.GetElapsedTime(started);

        output.WriteLine($"Provider accepted the message in {elapsed.TotalSeconds:F2}s (target 60s).");
        Assert.True(elapsed < TimeSpan.FromSeconds(60));
    }
}