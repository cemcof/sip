namespace sip.Utils.EntityLogging;

public class EntityLogger<TEntity>(
    string                                   categoryName,
    Action<TEntity>                          consumer,
    Func<LogLevel, string, string, TEntity>? standardLogFactory = null)
    : ILogger
{
    public IDisposable BeginScope<TState>(TState state) => default!;

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (state is TEntity tstate)
        {
            // Don't use formatters, that is for other loggers.
            consumer(tstate);
        }
        else if (standardLogFactory is not null)
        {
            var entity = standardLogFactory(logLevel, categoryName, formatter(state, exception));
            consumer(entity);
        }
        
    }
}