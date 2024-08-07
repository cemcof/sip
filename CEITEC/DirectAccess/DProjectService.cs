using sip.Projects.Statuses;

namespace sip.CEITEC.DirectAccess;

public class DProjectService(
        ProjectManipulationHelperService          project,
        TimeProvider                              timeProvider,
        IOptionsMonitor<StatusOptions>           statusOptions,
        ProjectStatusHelperService                status,
        YearOrderIdGeneratorService               idGen,
        ProjectOrganizationMessageBuilderProvider messageBuilderProvider)
    :
        IProjectDefine<DProject>,
        IManyProjectsLoader<DProject, DProjectFilter>,
        IProjectLoader<DProject>

{
    private readonly TimeProvider                              _timeProvider            = timeProvider;
    private readonly IOptionsMonitor<StatusOptions>           _statusOptions          = statusOptions;
    private readonly ProjectStatusHelperService                _status                 = status;
    private readonly ProjectOrganizationMessageBuilderProvider _messageBuilderProvider = messageBuilderProvider;



    public Task<ProjectLoadResults> LoadManyAsync(DProjectFilter? filter)
    {
        throw new NotImplementedException();
    }

    
    public Task<string> GenerateNextIdAsync()
        => idGen.GenerateNextIdAsync<DProject>(new YearOrderIdGeneratorOptions {Postfix = "D"});

    public Task<DProject> LoadAsync(string id)
        => project.LoadProjectAsync<DProject>(id);

    public Type ProjectType => typeof(DProject);
    public string DisplayName => "DirectAccess";
    public string Theme => "lightblue";
   
}

public class DProjectFilter : ProjectFilter, IProjectFilter<DProject>;
