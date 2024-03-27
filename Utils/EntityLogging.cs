using System.Collections.Concurrent;
using System.Diagnostics;

namespace sip.Utils;
public class EntityLoggerOptions
{
    public TimeSpan LogFlushInterval { get; set; } = TimeSpan.FromSeconds(5);
}

public class EntityLoggerProvider<TDbContext, TEntity>(
        IOptions<EntityLoggerOptions>            options,
        IDbContextFactory<TDbContext>            contextFactory,
        Func<LogLevel, string, string, TEntity>? standardLogFactory = null)
    : BackgroundQueue<TEntity>(options.Value.LogFlushInterval), ILoggerProvider
    where TDbContext : DbContext
{
    private readonly IOptions<EntityLoggerOptions>            _options            = options;

    private PropertyInfo? _keyProperty;
    private PropertyInfo KeyProperty
    {
        get
        {
            // Cache property so we avoid user reflection on every use
            if (_keyProperty is null)
            {
                using var dbctx = contextFactory.CreateDbContext();
                var entityType = dbctx.Model.FindEntityType(typeof(TEntity));
                Debug.Assert(entityType is not null);
                var primaryKey = entityType.FindPrimaryKey();
                Debug.Assert(primaryKey is not null);
                
                var keyName = primaryKey.Properties
                    .Select(x => x.Name)
                    .Single();
                
                Debug.Assert(!string.IsNullOrEmpty(keyName));
                _keyProperty = typeof(TEntity).GetProperty(keyName)!;
            }

            return _keyProperty;
        }
    }
    
    private readonly ConcurrentDictionary<string, ILogger> _loggersCache = new();


    protected override void Process()
    {
        if (ConcurrentQueue.IsEmpty)
            return;

        using var dbctx = contextFactory.CreateDbContext();
        
        var logs = new List<TEntity>();
        while (!ConcurrentQueue.IsEmpty)
        {
            // THIS WHILE LOOP MUST NOT PRODUCE ANY LOGS, INIFINITE LOOP OTHERWISE
            var deqResult = ConcurrentQueue.TryDequeue(out var item);
            if (!deqResult || item is null) return;
            Debug.Assert(typeof(TEntity) == item?.GetType());
            logs.Add(item);
        }
        
        // Now this is important - some logs might be duplicated and saving them to ctx throws an exception.
        logs = logs.DistinctBy(l => KeyProperty.GetValue(l, null)).ToList();

        foreach (var l in logs)
        {
            var primkey = KeyProperty.GetValue(l, null);

            if (dbctx.Find(l!.GetType(), primkey) is null)
            {
                // No such thing in the db yet, insert
                dbctx.Attach(l);
                dbctx.Entry(l).State = EntityState.Added;
            }
        }

        dbctx.SaveChanges();
    }
    
    public ILogger CreateLogger(string categoryName)
    {
        return _loggersCache.GetOrAdd(categoryName, name => new EntityLogger<TEntity>(categoryName, Add, standardLogFactory));
    }

}

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

public static class EntityLoggingExtensions
{
    public static void AddEntityLogger<TEntityLoggerProvider>(this IServiceCollection services, Action<EntityLoggerOptions> configure)
     where TEntityLoggerProvider: class, ILoggerProvider, IHostedService
    {
        
        services.Configure(configure);
        services.AddSingleton<TEntityLoggerProvider>();
        services.AddSingleton<ILoggerProvider>(s => s.GetRequiredService<TEntityLoggerProvider>());
        services.AddSingleton<IHostedService>(s => s.GetRequiredService<TEntityLoggerProvider>());
    }
}