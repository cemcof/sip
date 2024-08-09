using sip.Core;

namespace sip.Projects;

public interface IProjectItemRenderProvider
{
    ComponentRenderInfo GetProjectItemComponent(Project project);
}

public interface IProjectItemRenderProvider<in TProject> where TProject : Project
{
    ComponentRenderInfo GetProjectItemComponent(TProject project);
}

public class ProjectRenderProviderDelegatorService(
        IDbContextFactory<AppDbContext> dbContextFactory,
        IServiceProvider                serviceProvider)
    : IProjectItemRenderProvider
{
    public ComponentRenderInfo GetProjectItemComponent(Project project)
    { 
        var ptType = project.GetType();
        
        dynamic renderProvider = serviceProvider.GetRequiredService(typeof(IProjectItemRenderProvider<>).MakeGenericType(ptType));
        return renderProvider.GetProjectItemComponent(project);
    }
    

    public async Task<Type> GetProjectComponent(string pid)
    {
        // Get project type 
        await using var db = await dbContextFactory.CreateDbContextAsync();
        
        var p = await db.Set<Project>().FirstOrDefaultAsync(p => p.Id == pid);
        if (p is null) return typeof(ProjectNotFoundComponent);
        
        var projType = p.GetType();

        dynamic renderProvider = serviceProvider.GetRequiredService(typeof(IProjectItemRenderProvider<>).MakeGenericType(projType));
        return renderProvider.GetProjectComponent();
    }
}