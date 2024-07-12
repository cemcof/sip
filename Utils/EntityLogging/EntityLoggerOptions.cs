namespace sip.Utils.EntityLogging;

public class EntityLoggerOptions
{
    public TimeSpan LogFlushInterval { get; set; } = TimeSpan.FromSeconds(5);
}