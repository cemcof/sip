namespace sip.Scheduling;

public static class SchedulingExtensions
{
    public static void AddScheduledService<TScheduledService>(this IServiceCollection services, Action<ScheduledServiceOptions> configure) 
        where TScheduledService : ScheduledService
    {
        
        // Options
        var optsname = typeof(TScheduledService).Name;
        services.Configure(optsname, configure);
        
        // Hosted service
        services.AddSingleton<TScheduledService>();
        services.AddSingleton<IHostedService>(s => s.GetRequiredService<TScheduledService>());
    }
    
    public static void AddScheduledService<TScheduledService>(this IServiceCollection services, IConfiguration conf) 
        where TScheduledService : ScheduledService
    {
        
        // Options
        var optsname = typeof(TScheduledService).Name;
        services.Configure<ScheduledServiceOptions>(optsname, conf);
        
        // Hosted service
        services.AddSingleton<TScheduledService>();
        services.AddSingleton<IHostedService>(s => s.GetRequiredService<TScheduledService>());
    }
}