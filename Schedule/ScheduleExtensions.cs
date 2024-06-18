using sip.Scheduling;
using sip.Utils.Crm;
using sip.Experiments;

namespace sip.Schedule;

public static class ScheduleExtensions
{
    public static void AddSchedule(this IServiceCollection services, IConfigurationRoot config)
    {
        services.AddScheduledService<ScheduleService>(c =>
        {
            c.InitRun = true;
            c.Cron = "0 */15 * * * *";
            c.InitDueTime = TimeSpan.FromSeconds(10);
        });
        
        services.AddSingleton<IReservationsProvider>(s => s.GetRequiredService<ScheduleService>());

        services.AddCrm(config);
        services.AddSingleton<CrmReservationsEngine>();
        services.AddSingleton<IScheduleEngine>(s => s.GetRequiredService<CrmReservationsEngine>());
        
        services.AddOptions<ScheduleOptions>()
            .GetOrganizationOptionsBuilder(config)
            .BindOrganizationConfiguration("Schedule")
            .ConfigureWithOptionsDependency<InstrumentsOptions>((c, _,  io) =>
            {
                c.ReservationSubjects = io.Instruments.ToList();
            });
    }
}