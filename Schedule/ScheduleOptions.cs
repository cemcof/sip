using sip.Experiments;

namespace sip.Schedule;

public class ScheduleOptions
{
    [Required] public string Engine { get; set; } = null!;

    public int DaysPast { get; set; } = 4 * 7;
    public List<IInstrument> ReservationSubjects { get; set; } = new();
}