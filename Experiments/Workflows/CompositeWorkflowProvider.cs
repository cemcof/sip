namespace sip.Experiments.Workflows;

public class CompositeWorkflowProvider(IServiceProvider serviceProvider) : IWorkflowProvider 
{
    public async IAsyncEnumerable<Workflow> GetWorkflowsAsync(WorkflowFilter workflowFilter)
    {
        var providers = serviceProvider.GetRequiredService<IEnumerable<IWorkflowProvider>>()
            .Where(wp => wp is not CompositeWorkflowProvider);


        foreach (var workflowProvider in providers)
        {
            await foreach (var p in workflowProvider.GetWorkflowsAsync(workflowFilter))
            {
                yield return p;  
            }
        }
    }

    public async Task<Workflow?> GetWorkflowByIdAsync(string id, IOrganization organization)
    {
        var providers = serviceProvider.GetRequiredService<IEnumerable<IWorkflowProvider>>()
            .Where(wp => wp is not CompositeWorkflowProvider);

        foreach (var workflowProvider in providers)
        {
            var wf = await workflowProvider.GetWorkflowByIdAsync(id, organization);
            if (wf is not null)
            {
                return wf;
            }
        }

        return null;
    }
}