namespace sip.Organizations.Centers;

public class CentersOptions
{
    public List<CenterOptions> Centers { get; set; } = new();
    public TimeSpan KillAfterInactive { get; set; } = TimeSpan.FromDays(1);
}