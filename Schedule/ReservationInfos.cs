namespace sip.Schedule;

public struct ReservationInfosResult
{
    public List<(string subject, DateTime from, DateTime til)> 
        Subjects { get; set; } = new();

    public List<(string subject, DateTime @from, DateTime til)> Current => ForTimeMoment(DateTime.UtcNow);
    public void AddEntry(string subject, DateTime from, DateTime until)
    {
        Subjects.Add((subject, from, until));
    }

    public static ReservationInfosResult None()
    {
        return new ReservationInfosResult();
    }
    
    public ReservationInfosResult() { }


    public List<(string subject, DateTime @from, DateTime til)> ForTimeMoment(DateTime moment)
    {
        var relevants = Subjects
            .Where(x => x.from <= moment && x.til >= moment)
            // .DistinctBy(x => x.subject)
            .ToList();
        return relevants;
    }
}
