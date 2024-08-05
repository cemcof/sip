using sip.Experiments.Model;

namespace sip.Experiments;

public class ExperimentsOptions
{
    public Dictionary<IInstrument, Dictionary<string,ExperimentOptions>> InstrumentJobs { get; set; } = new();
    
    public ExperimentOptions FindExpOpts(string instrumentName, string technique) 
        => InstrumentJobs.First(kv => kv.Key.Name == instrumentName).Value[technique];
    
    public string FindTheme(Experiment experiment)
    {   
        // Find instrument
        var instjob = InstrumentJobs.First(kv => kv.Key.Name == experiment.InstrumentName);
        return instjob.Key.DisplayTheme;
        
        // TODO - also consider technique theme ?
        // Find technique
        // instjob.Value.TryGetValue(experiment.Technique, out var technique);
    } 
}
