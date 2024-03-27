using sip.Scheduling;
using sip.Experiments;

namespace sip.RemoteAccess;

public static class RemoteAccessExtensions
{
    public static IServiceCollection AddRemoteAccess(this IServiceCollection services, IConfigurationRoot config)
    {
        services.AddScheduledService<RemoteAccessService>(c => c.Cron = "*/5 * * * * *");
        services.AddSingleton<IRemoteAccess>(s => s.GetRequiredService<RemoteAccessService>());

        services.AddOptions<RemoteAccessOptions>()
            .GetOrganizationOptionsBuilder()
            .ConfigureWithOptionsDependency<InstrumentsOptions>((ro, conf, io) =>
            {
                foreach (var instConf in conf.GetSection("RemoteAccess:Instruments").GetChildren())
                {
                    ro.Instruments.Add(
                        io.GetInstrumentByName(instConf.Key),
                        instConf.Get<List<RemoteConnectionInfo>>()
                        ?? throw new InvalidOperationException(
                            $"Cannot configure list of remote configure options for {instConf.Key}")
                    );
                }
            });
        
        // Guacamole
        services.AddOptions<GuacamoleOptions>()
            .GetOrganizationOptionsBuilder()
            .BindOrganizationConfiguration(config, "Guacamole");
        
        services.AddSingleton<GuacamoleDriver>();
        return services;
    }
}
