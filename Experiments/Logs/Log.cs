using System.ComponentModel.DataAnnotations.Schema;
using sip.Experiments.Model;

namespace sip.Experiments.Logs;

public class Log : IComparable<Log>
{
    public Guid Id { get; set; }
        
    public Experiment? Experiment { get; set; }
    public Guid? ExperimentId { get; set; }

    public DateTime Dt { get; set; }
    public string Origin { get; set; } = "Gui";
    public LogLevel Level { get; set; }

    [NotMapped] public string PLevel
    {
        set
        {
            Level = value switch
            {
                "DEBUG" => LogLevel.Debug,
                "CRITICAL" => LogLevel.Critical,
                "WARNING" => LogLevel.Warning,
                "ERROR" => LogLevel.Error,
                "INFO" => LogLevel.Information,
                _ => LogLevel.None
            };
        }
        get
        {
            return Level switch
            {
                LogLevel.Debug => "DEBUG",
                LogLevel.Critical => "CRITICAL",
                LogLevel.Warning => "WARNING",
                LogLevel.Error => "ERROR",
                LogLevel.Information => "INFO",
                _ => "NONE"
            };
        }
    }

    public string Message { get; set; } = string.Empty;

    public static Log FromStandardLog(LogLevel level, string category, string message)
    {
        var l = new Log()
        {
            Dt = DateTime.UtcNow,
            Origin = category,
            Message = message,
            Level = level
        };

        return l;
    }

    public int CompareTo(Log? other)
    {
        if (other is null) return 1;
        if (other.Id == Id) return 0;
        if (other.Dt == Dt) return -1;
        return Comparer<DateTime>.Default.Compare(other.Dt, Dt);
    }
}