using sip.Userman;
using sip.Utils;

namespace sip.Projects.Markers;

public record MarkerInfo(
    string Name,
    string Description,
    AttentionLevel AttentionLevel
    );

public interface IProjectMarker<TProject> where TProject : Project
{
    IQueryable<TProject> FilterHelper(TProject project);
    

    Task<bool> IsAsync(AppUser user, TProject project);
    
    MarkerInfo GetMarkerInfo();
}


// public class ProjectMarker : IProjectMarker<Project>
// {
//     public ProjectMarker()
//     {
//         IQueryable<Project> projects;
//         projects.AsQueryable()
//     }
// }
