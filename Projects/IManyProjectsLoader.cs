namespace sip.Projects;

public interface IManyProjectsLoader<TProject, in TFilter>
    where TProject : Project
    where TFilter : IProjectFilter<TProject>
{
    Task<ProjectLoadResults> LoadManyAsync(TFilter? filter);
}

public interface IProjectLoadResults;

public interface IProjectLoadResults<TProject>
{
    public List<TProject> Projects { get; set; }
}

public record ProjectLoadResults(int TotalCount, IEnumerable<Project> Items) : IFilteredResult<Project>;