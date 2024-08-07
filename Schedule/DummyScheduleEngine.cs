using sip.Experiments;

namespace sip.Schedule;

public class DummyReservationsEngine : IScheduleEngine
{
    public Task<ScheduleData> GetReservationsDataAsync(IOrganization organization)
        => Task.FromResult<ScheduleData>(new ScheduleData(DateTime.UtcNow, new List<ScheduleInstrument>()));

    public Task RefreshAsync(IOrganization organization, IEnumerable<IInstrument> subjects, int daysPast)
        => Task.CompletedTask;
}