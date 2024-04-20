namespace sip.Experiments.Workflows;

public class FromConfigWorkflowProvider(IOptionsMonitor<List<Workflow>> wfOptions) : IWorkflowProvider
{
    public IAsyncEnumerable<Workflow> GetWorkflowsAsync(WorkflowFilter workflowFilter)
    {
        var wfs = wfOptions.Get(workflowFilter.Organization).AsEnumerable()
            .Where(wf => workflowFilter.Tags.Match(wf.Tags));
        
        return wfs.ToAsyncEnumerable();
    }

    public Task<Workflow?> GetWorkflowByIdAsync(string id, IOrganization organization)
    {
        var opts = wfOptions.Get(organization);
        var wf = opts.FirstOrDefault(w => w.Id == id);
        return Task.FromResult(wf);
    }
} 