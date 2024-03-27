using sip.Experiments.Model;

namespace sip.Experiments;

public interface IExperimentHandler
{
    Task RequestStart(Experiment experiment);
    Task RequestStop(Experiment  experiment);
}