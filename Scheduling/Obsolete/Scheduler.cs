namespace sip.Scheduling.Obsolete;

public class Scheduler(ILogger logger) : ISchedulerStarter
{
    public virtual async Task Run()
    {
        while (true)
        {
            try
            {
                var nextRun = ComputeDelay();
                logger.LogInformation("Next execution scheduled to: {cd} (after {timespan})", DateTime.Now + nextRun, nextRun); 
                await Task.Delay(nextRun);
                logger.LogInformation("Executing action");
                await ExecAction();
                logger.LogInformation("Action execution finished successfully");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception occured during scheduled action execution");
            }
        }
    }

    public virtual Task ExecAction()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Computes delay after which next action execution of this scheduler will happen.
    /// </summary>
    /// <returns>TimeSpan representing the delay.</returns>
    protected virtual TimeSpan ComputeDelay()
    {
        return TimeSpan.FromHours(1);
    }
}