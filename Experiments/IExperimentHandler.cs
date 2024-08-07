using sip.Experiments.Logs;
using sip.Experiments.Model;

namespace sip.Experiments;

public interface IExperimentHandler
{
    Task RequestStart(Experiment experiment);
    Task RequestStop(Experiment experiment, ExperimentStopModel stopModel);

    event Action<IReadOnlyCollection<Log>> ExperimentLogAdded;
}