namespace sip.Experiments.Workflows;

public class FromConfigWorkflowProvider(IOptionsMonitor<List<Workflow>> wfOptions) : IWorkflowProvider
{
    public IAsyncEnumerable<Workflow> GetWorkflowsAsync(WorkflowFilter workflowFilter)
    {
        var wfs = wfOptions.Get(workflowFilter.Organization).AsEnumerable();
        
        workflowFilter.ta
        
        if (workflowFilter.Technique is not null)
        {
            wfs = wfs.Where(w => w.Tags.Contains(workflowFilter.Technique));
        }
        
        if (workflowFilter.Instrument is not null)
        {
            wfs = wfs.Where(w => w.Tags.Contains(workflowFilter.Instrument));
        }
        
        if (workflowFilter.Engine is not null)
        {
            wfs = wfs.Where(w => w.Tags.Contains(workflowFilter.Engine));
        }
        
        return wfs.ToAsyncEnumerable();
    }

    public Task<Workflow?> GetWorkflowByIdAsync(string id, IOrganization organization)
    {
        var opts = wfOptions.Get(organization);
        var wf = opts.FirstOrDefault(w => w.Id == id);
        return Task.FromResult(wf);
    }
} 