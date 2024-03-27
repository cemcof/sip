namespace sip.Experiments;

public class ExperimentsOptions
{
    public Dictionary<IInstrument, Dictionary<string,ExperimentOptions>> InstrumentJobs { get; set; } = new();
}
