using Microsoft.Extensions.Internal;
using sip.Experiments;
using sip.Scheduling;
using IOrganization = sip.Organizations.IOrganization;
using IOrganizationProvider = sip.Organizations.IOrganizationProvider;

namespace sip.Schedule;

public interface IScheduleEngine : IReservationsProvider
{
    Task RefreshAsync(IOrganization organization, IEnumerable<IInstrument> subjects, int daysPast);
}

public class ScheduleService(
        ILogger<ScheduleService> logger,
        IEnumerable<IScheduleEngine> scheduleEngines,
        IOptionsMonitor<ScheduleOptions> scheduleOptions,
        IOptionsMonitor<ScheduledServiceOptions> scheduleServiceOptions,
        ISystemClock systemClock,
        IOrganizationProvider organizationProvider)
    : ScheduledService(scheduleServiceOptions, systemClock, logger), IReservationsProvider
{
    // private void FetchScheduleData()

    // private string InstrumentCRMRemap(string instrument)
    // {
    //     // For now, it is just required to remove space from intrument CRM name and it will match the names within the system
    //     // Talos Arctica -> TalosArctica
    //     // Titan Krios -> TitanKrios
    //     // etc... 
    //     return instrument.Replace(" ", "");
    // }


    protected override async Task ExecuteRoundAsync(CancellationToken stoppingToken)
    {
        Logger.LogDebug("Executing sched service");
        var organizations = organizationProvider.GetAll();
        foreach (var organization in organizations)
        {
            try
            {
                var scheduleOrgOptions = scheduleOptions.Get(organization);
                var engine = FindEngine(scheduleOrgOptions.Engine);
                if (engine is not null)
                {
                    await engine.RefreshAsync(organization, scheduleOrgOptions.ReservationSubjects,
                        scheduleOrgOptions.DaysPast);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to refresh reservations of {}", organization);
            }
        }
    }

    private IScheduleEngine? FindEngine(string name)
        => scheduleEngines.FirstOrDefault(e => e.GetType().Name == name);

    public Task<ScheduleData> GetReservationsDataAsync(IOrganization organization)
    {
        var scheduleEngine = scheduleOptions.Get(organization).Engine;
        var engine = FindEngine(scheduleEngine) ??
                     throw new InvalidOperationException($"Organization {organization.Id} does " +
                                                         $"not have a valid schedule engine configured ({scheduleEngine})");

        return engine.GetReservationsDataAsync(organization);
    }
}