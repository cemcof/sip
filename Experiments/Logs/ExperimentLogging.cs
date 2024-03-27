using sip.Core;
using sip.Experiments.Model;
using sip.Utils;

namespace sip.Experiments.Logs;

[ProviderAlias("ExperimentLogging")]
public class ExperimentLoggingProvider(
        IOptions<EntityLoggerOptions>   options,
        IDbContextFactory<AppDbContext> contextFactory)
    : EntityLoggerProvider<AppDbContext, Log>(options, contextFactory, Log.FromStandardLog);

public static class ExperimentLoggingExtensions
{
    public static void LogExp(this ILogger logger, Log log)
    {
        logger.Log(log.Level, 0, log, null, LogExpFormatter);
    }
        
    public static void LogExp(this ILogger logger, LogLevel loglevel, Experiment exp, string message, params object[] args)
    {
        var logstate = new Log()
        {
            ExperimentId = exp.Id,
            Dt = DateTime.UtcNow,
            Level = loglevel,
            Message = string.Format(message, args)
        };
            
        logger.Log(loglevel, 0, logstate, null, LogExpFormatter);
    }

    private static string LogExpFormatter(Log logstate, Exception? ex = null)
    {
        var expjobinfo = (logstate.Experiment is not null)
            ? logstate.Experiment.Technique + $"({logstate.Experiment.SecondaryId})" : "";
        var result = $"{logstate.Origin} {expjobinfo} {logstate.Message}";
        return result;
    }

}