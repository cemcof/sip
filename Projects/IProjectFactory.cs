namespace sip.Projects;

public interface IProjectFactory<TProject> where TProject : Project
{
    Task<TProject> CreateProjectAsync();
}