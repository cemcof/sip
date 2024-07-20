using Cronos;

namespace sip.Scheduling;

public enum ScheduledRunStrategy
{
    /// <summary>
    /// Skips the scheduled action previous is still running
    /// </summary>
    SkipIfRunning,
    /// <summary>
    /// Wait for the previous action to finish, then run the new scheduled action
    /// </summary>
    QueueIfRunning,
    /// <summary>
    /// If previous action is still running, cancel it and start the new one
    /// </summary>
    CancelIfRunning,
    /// <summary>
    /// Ignore previous running action and start a new one in parallel
    /// </summary>
    RunInParallelIfRunning
}

public class ScheduledServiceOptions
{
    /// <summary>
    /// Cron expression for the scheduled run in format including seconds
    /// User either this or specify <see cref="ScheduledServiceOptions.Interval"/>
    /// </summary>
    public string? CronString
    {
        get => Cron?.ToString();
        set => Cron = CronExpression.Parse(value, CronFormat.IncludeSeconds);
    }

    public CronExpression? Cron { get; set; }
    
    /// <summary>
    /// Interval between scheduled runs.
    /// Use either this or specify <see cref="ScheduledServiceOptions.Cron"/>
    /// </summary>
    public TimeSpan Interval { get; set; }
    
    public ScheduledRunStrategy SkipRoundIfRunning { get; set; }
    public bool Enabled { get; set; } = true;

    
    /// <summary>
    /// If set, runs one round when application starts and after finishing it starts the cron
    /// </summary>
    public bool InitRun { get; set; }

    /// <summary>
    /// After which time to start executing the cron
    /// </summary>
    public TimeSpan InitDueTime { get; set; }
}