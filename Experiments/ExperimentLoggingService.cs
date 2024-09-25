using sip.Core;
using sip.Experiments.Logs;
using sip.Experiments.Model;

namespace sip.Experiments;

public class ExperimentLoggingService(ILogger<ExperimentLoggingService> logger, IDbContextFactory<AppDbContext> dbContextFactory)
{
    public async Task SubmitLogAsync(
        Experiment        exp,
        LogLevel          level,
        string            message,
        CancellationToken ct = default)
    {
        var log = new Log
        {
            Id           = Guid.NewGuid(),
            Dt           = DateTime.UtcNow,
            ExperimentId = exp.Id,
            Message      = message,
            Origin       = "LIMS Webserver",
            Level        = level
        };
        
        await SubmitLogsAsync([log], ct);
    }

    public async Task SubmitLogsAsync(List<Log> logs, CancellationToken ct)
    {
        if (!logs.Any()) return;

        await using var context = await dbContextFactory.CreateDbContextAsync(ct);
        
        // For now, UPSERT logs one by one 
        var logsSet = context.Set<Log>();
        
        foreach (var log in logs)
        {
            try
            {
                var hasLog = await logsSet.CountAsync(l => log.Id == l.Id, cancellationToken: ct);
                if (hasLog == 0)
                {
                    logsSet.Add(log);
                }
                else
                {
                    logsSet.Update(log);
                }

                await context.SaveChangesAsync(ct);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to save experiment log: {@Log}", log);       
            }
        }

        ExperimentLogAdded?.Invoke(logs);
    }

    
    public event Action<IReadOnlyCollection<Log>>? ExperimentLogAdded;
    
}