using Microsoft.Extensions.Internal;

namespace sip.Scheduling;

/// <summary>
/// Scheduled service used for testing, just log
/// </summary>
public class TestScheduledService(
        IOptionsMonitor<ScheduledServiceOptions> optionsMonitor,
        TimeProvider                             timeProvider,
        ILogger<TestScheduledService>            logger)
    : ScheduledService(optionsMonitor, timeProvider, logger)
{
    protected override async Task ExecuteRoundAsync(CancellationToken stoppingToken)
    {
        Logger.LogInformation("Executing round async, this will take 10 seconds");
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        Logger.LogInformation("Finished round async");
    }
}