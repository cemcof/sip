using sip.Core;
using sip.Projects.Statuses;

namespace sip.Projects;

public class ProjectStatusHelperService(IDbContextFactory<AppDbContext> dbContextFactory,
                                        TimeProvider                    timeProvider,
                                        IOptionsMonitor<StatusOptions> statusOptionsMonitor,
                                        IOrganizationProvider          organizationProvider)
{
    public Task ChangeStatusAsync<TStatusRef>(Project project, IEnumerable<string> organizationIds)
        => ChangeStatusAsync(project, typeof(TStatusRef).Name, organizationIds);
    
    public async Task ChangeStatusAsync(Project project, string statusId, IEnumerable<string> organizationIds)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var pentry = ctx.Attach(project);
        await pentry.Collection(p => p.ProjectStatuses)
            .LoadAsync();

        
        // We manage statuses only on leaf level nodes
        var targetOrgs = new List<Organization>();
        foreach (var orgid in organizationIds)
        {
            var rorog = organizationProvider.GetFromString(orgid);
            targetOrgs.AddRange(new Tree<Organization>(rorog).EnumerateLeaves());
        }
            
        
        foreach (var org in targetOrgs.Distinct())
        {
            // Get active status
            var enteredFromStatus = project.Statuses.FirstOrDefault(s => s.Active && s.OrganizationId == org.Id);
            // Skip if statuses are the same
            if (enteredFromStatus is not null && enteredFromStatus.StatusInfoId == statusId) continue;
            // Otherwise, create new status entry
            var status = new Status()
            {
                Active = true, 
                DtEntered = timeProvider.DtUtcNow(),
                OrganizationId = org.Id,
                StatusInfoId = statusId,
                EnteredFromStatus = enteredFromStatus,
                               
            };
            // Update entry we are entering from a status
            if (enteredFromStatus is not null)
            {
                enteredFromStatus.LeftToStatus = status;
                enteredFromStatus.Active = false;
                enteredFromStatus.DtLeft = timeProvider.DtUtcNow();
            }
            
            project.ProjectStatuses.Add(status);
        }
        
        // Eventually, save changes
        await ctx.SaveChangesAsync();
    }
    

    public Task InStatus<TStatusRef, TOrganizationRef>(Project project)
    {
        throw new NotImplementedException();
    }

    public StatusInfo GetStatusInfo<TStatusRef>() where TStatusRef : StatusDefinition
    {
        var sr = (StatusDefinition)Activator.CreateInstance(typeof(TStatusRef))!;
        return statusOptionsMonitor.Get(sr.Name).StatusDetails;
    }

}