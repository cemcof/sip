namespace sip.Experiments.Workflows;

public class FromConfigWorkflowProvider : IWorkflowProvider
{
    public IAsyncEnumerable<Workflow> GetWorkflowsAsync(ExperimentOptions forExp)
    {
        return forExp.Workflows.Values.ToAsyncEnumerable();
    }

    public Task<Workflow?> GetWorkflowByIdAsync(string id)
    {
        throw new NotImplementedException();
    }
}