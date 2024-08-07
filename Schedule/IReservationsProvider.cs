namespace sip.Schedule;

public interface IReservationsProvider
{
    Task<ScheduleData> GetReservationsDataAsync(IOrganization organization);
}