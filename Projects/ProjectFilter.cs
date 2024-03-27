using Newtonsoft.Json;
using sip.Organizations;
using sip.Projects.Statuses;
using sip.Userman;
using sip.Utils.Items;

namespace sip.Projects;

public interface IProjectFilter<TProject>;

public interface IProjectResults<TProject>;

public interface IProjectsFilterProvider
{
    Task<ProjectsFilter> GetProjectFiltersAsync(ClaimsPrincipal user);
}

public interface IProjectFilterProvider<TProject>
{
    Task GetProjectFiltersAsync(ClaimsPrincipal user);
}

public class ProjectFilter : IFilter
{
    public DateTime Since { get; set; }
    public DateTime Until { get; set; }

    public List<string> OrganizationIds { get; set; } = new();
    public List<string> StatusIds { get; set; } = new();
    
    public AppUser? ForUser { get; set; }
    public string? FilterQuery { get; set; } = null;
    public int Count { get; set; } = -1;
    public int Offset { get; set; } = 0;
    public CancellationToken CancellationToken { get; set; } = default;
}




public class ProjectsFilter
{
    public List<Organization> PossibleOrganizations { get; set; } = new();
    public List<Organization> Organizations { get; set; } = new();
    
    public List<StatusInfo> PossibleStatuses { get; set; } = new();
    public List<StatusInfo> Statuses { get; set; } = new();
    
    [JsonIgnore] public string SearchString { get; set; } = string.Empty;
    [JsonIgnore] public ClaimsPrincipal? CpRequester { get; set; }
    public List<ProjectFilter> ProjectFilters { get; set; } = new();
    public AppUser? UserRequester { get; set; }
}


public class ProjectResults
{
    public int Count { get; set; }
}

