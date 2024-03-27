namespace sip.Projects;

public class ProjectsFilterProvider : IProjectsFilterProvider
{
    public Task<ProjectsFilter> GetProjectFiltersAsync(ClaimsPrincipal user)
    {
        // TODO -
        return Task.FromResult(new ProjectsFilter());
    }
}