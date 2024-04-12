namespace sip.Experiments.Workflows;

public interface IWorkflowProvider
{
    IAsyncEnumerable<Workflow> GetWorkflowsAsync(WorkflowFilter? filter = null);
    Task<Workflow?> GetWorkflowByIdAsync(string id);
}