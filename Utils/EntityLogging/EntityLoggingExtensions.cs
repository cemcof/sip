namespace sip.Utils.EntityLogging;

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