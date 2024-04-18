namespace sip.Experiments.Workflows;

public interface IWorkflowProvider
{
    IAsyncEnumerable<Workflow> GetWorkflowsAsync(WorkflowFilter filter);
    Task<Workflow?> GetWorkflowByIdAsync(string id, IOrganization organization);
}