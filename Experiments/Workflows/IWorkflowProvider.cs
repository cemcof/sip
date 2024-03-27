namespace sip.Experiments.Workflows;

public interface IWorkflowProvider
{
    IAsyncEnumerable<Workflow> GetWorkflowsAsync(ExperimentOptions forExp);
    Task<Workflow?> GetWorkflowByIdAsync(string id);
}