using sip.Core;

namespace sip.Projects;

/// <summary>
/// Delegates loading of projects of unspecified type to concrete typed project loaders,
/// obtained dynamically from service provider
/// </summary>
public class ProjectLoadingDelegator(
        IServiceProvider                 serviceProvider,
        IDbContextFactory<AppDbContext>  dbContextFactory,
        ILogger<ProjectLoadingDelegator> logger,
        IEnumerable<IProjectDefine>      projectDefines)
    : IProjectLoader
{
    private readonly IEnumerable<IProjectDefine>      _projectDefines   = projectDefines;

    public async Task<Project?> LoadAsync(string id)
    {
        // We need to obtain the actual type of the project first, do simple load
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var p = await db.Set<Project>().FirstOrDefaultAsync(p => p.Id == id);

        if (p is not null)
        {
            logger.LogDebug("Delegator loaded project of type {} for id {}", p.GetType().Name, id);
            
            var loader = serviceProvider.GetRequiredService(typeof(IProjectLoader<>).MakeGenericType(p.GetType()));
            var loaderMethod = loader.GetType().GetMethod(nameof(IProjectLoader<Project>.LoadAsync))!;
            dynamic task = loaderMethod.Invoke(loader, new object?[] {id})!;
            return await task;
        }

        return null;
    }

    public async Task<ProjectsLoadResults> LoadManyAsync(ProjectsFilter filter)
    {
        // Fetch and accumulate results, sort them by dt for now
        var result = new ProjectsLoadResults();
        
        foreach (var filterProjectFilter in filter.ProjectFilters)
        {
            filterProjectFilter.FilterQuery = filter.SearchString;
            filterProjectFilter.ForUser = filter.UserRequester;
            
            // Extract project type this filter implements
            var itterf = filterProjectFilter.GetType().GetInterfaces()
                .First(i => i.GetGenericTypeDefinition() == typeof(IProjectFilter<>));

            var projType = itterf.GenericTypeArguments.First();

            
            object projLoader =
                serviceProvider.GetRequiredService(
                    typeof(IManyProjectsLoader<,>).MakeGenericType(projType, filterProjectFilter.GetType()));

            dynamic task = projLoader.GetType().GetMethod("LoadManyAsync")!
                .Invoke(projLoader, new object?[] {filterProjectFilter})!;
            ProjectLoadResults ldResult = await task;
            
            result.ProjectLoadResults.Add(ldResult);
        }

        return result;
        
        // await using var db = await _dbContextFactory.CreateDbContextAsync();
        //
        // foreach (var projectDefine in _projectDefines)
        // {
        //     // dynamic loader = _serviceProvider.GetRequiredService(
        //     //     typeof(IManyProjectsLoader<>).MakeGenericType(projectDefine.ProjectType.ProjectType));
        //
        //     // var _ = nameof(IProjectLoader<Project>.LoadAsync);
        //     // await loader.LoadAsync();
        //
        // }
        //
        //
        //
        //
        //
        // throw new NotImplementedException();
    }
}