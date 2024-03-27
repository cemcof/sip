namespace sip.Autoloaders;

public class AutoloadersOptions
{
    public Dictionary<string, string> InstrumentData { get; set; } = new();
    public List<string> SystemValues { get; set; } = new();
    public List<string> GridUsageItems { get; set; } = new();
    public Dictionary<string, string> InitialState { get; set; } = new();
    public TimeSpan MinimalTimeToReport { get; set; } = TimeSpan.FromHours(1);
    public List<int> Positions { get; set; } = new();
}

