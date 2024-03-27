namespace sip.Experiments.Workflows;

public class CompositeWorkflowProvider(IServiceProvider serviceProvider) : IWorkflowProvider 
{
    public async IAsyncEnumerable<Workflow> GetWorkflowsAsync(ExperimentOptions forExp)
    {
        var providers = serviceProvider.GetRequiredService<IEnumerable<IWorkflowProvider>>()
            .Where(wp => wp is not CompositeWorkflowProvider);


        foreach (var workflowProvider in providers)
        {
            await foreach (var p in workflowProvider.GetWorkflowsAsync(forExp))
            {
                yield return p;  
            }
        }
    }

    public async Task<Workflow?> GetWorkflowByIdAsync(string id)
    {
        var providers = serviceProvider.GetRequiredService<IEnumerable<IWorkflowProvider>>()
            .Where(wp => wp is not CompositeWorkflowProvider);

        foreach (var workflowProvider in providers)
        {
            var wf = await workflowProvider.GetWorkflowByIdAsync(id);
            if (wf is not null)
            {
                return wf;
            }
        }

        return null;
    }
}