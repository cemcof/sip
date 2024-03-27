namespace sip.Projects;

public class DefaultProjectFactory<TProject> : IProjectFactory<TProject> where TProject : Project
{
    public Task<TProject> CreateProjectAsync()
    {
        throw new NotImplementedException();
    }
}