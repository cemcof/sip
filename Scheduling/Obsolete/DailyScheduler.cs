namespace sip.Scheduling.Obsolete;

public class DailySchedulerOptions
{
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public TimeSpan TickTime { get; set; }
}

public class DailyScheduler(IOptions<DailySchedulerOptions> options, ILogger logger) : Scheduler(logger)
{
    readonly DailySchedulerOptions _options = options.Value;

    protected override TimeSpan ComputeDelay()
    {
        var today = DateTime.UtcNow;
        var targetToday = today.Date + _options.TickTime;
        if ((targetToday - today).TotalSeconds < 120) // Schedule for the next day - note the tolerance
        {
            targetToday = targetToday.AddDays(1);
        }

        return targetToday - today;
    }
}