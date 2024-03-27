namespace sip.Projects;


public class ProjectsLoadResults
{
    public List<ProjectLoadResults> ProjectLoadResults { get; set; } = new();

    public IEnumerable<Project> Projects => ProjectLoadResults.SelectMany(ps => ps.Items);
}

public interface IProjectLoader
{
    Task<Project?> LoadAsync(string id);
    Task<ProjectsLoadResults> LoadManyAsync(ProjectsFilter filter);
}

public interface IProjectLoader<TProject> where TProject : Project
{
    Task<TProject> LoadAsync(string id);

}