using Cronos;
using Microsoft.Extensions.Internal;

namespace sip.Scheduling;

public abstract class ScheduledService(
        IOptionsMonitor<ScheduledServiceOptions> optionsMonitor,
        TimeProvider                             timeProvider,
        ILogger                                  logger)
    : BackgroundService
{
    protected readonly TimeProvider                             timeProvider     = timeProvider;
    protected readonly ILogger                                  Logger          = logger;

    private ScheduledServiceOptions Opts => optionsMonitor.Get(GetType().Name) ?? optionsMonitor.CurrentValue;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Helper execution method
        async Task<bool> Exec()
        {
            try
            {
                await ExecuteRoundAsync(stoppingToken);
            }
            catch (TaskCanceledException)
            {
                Logger.LogWarning("Scheduled task cancelled during execution");
                return false;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Unhandled exception during scheduled task execution");
            }

            return true;
        }
        
        // Wait for some time if desired
        if (Opts.InitDueTime != default)
        {
            try
            {
                await Task.Delay(Opts.InitDueTime, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                return;
            }
        }

        // Perform initial run if desired 
        if (Opts.InitRun)
        {
            if (!await Exec()) return;
        }
        
        // Start the execution loop
        while (true)
        {
            var opts = Opts;
            if (opts.Interval == default && opts.Cron == null)
                throw new ArgumentException("Either Interval or Cron must be specified");
            
            if (stoppingToken.IsCancellationRequested) return;
            
            var ts = opts.Interval;
            var now = timeProvider.DtUtcNow();

            if (ts == default)
            {
                // Lets wait a bit to avoid busy loop since getting next occurence does not work for as expected
                await Task.Delay(TimeSpan.FromMilliseconds(400), stoppingToken);
                now = timeProvider.DtUtcNow();
                var cronExpr = CronExpression.Parse(opts.Cron, CronFormat.IncludeSeconds);
                var nextSched = cronExpr.GetNextOccurrence(now);
                if (!nextSched.HasValue) return;
                ts = nextSched.Value - now;
                Logger.LogDebug("Scheduled next cron ({}) execution to {}, which is after {}", 
                    opts.Cron, nextSched, ts);
            }
            else
            {
                Logger.LogDebug("Scheduled next interval ({}) execution to {} which is after {}", 
                    opts.Interval, now + opts.Interval, ts);
            }
            
            
            // Wait until the next iteration happens
            try
            {
                await Task.Delay(ts, stoppingToken);
            }
            catch (System.ArgumentException)
            {
                // Meybe this was fixed by not calling DtUtcNow again
                Logger.LogError("! BAD cron -  now={} ts={}", now, ts);
                throw;
            }
            
            if (!await Exec()) break;
        }
    }

    protected abstract Task ExecuteRoundAsync(CancellationToken stoppingToken);
    
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Scheduled service {} is stopping", GetType().Name);
        return base.StopAsync(cancellationToken);
    }
}