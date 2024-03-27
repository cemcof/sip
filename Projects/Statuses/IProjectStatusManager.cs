namespace sip.Projects.Statuses;

public interface IProjectStatusManager<in TProject>
{
    Task ChangeStatusAsync(TProject project, string statusId, IEnumerable<string> organizationIds);
    
    Task<IEnumerable<StatusInfo>> GetRelevantStatusInfosAsync(TProject project);
    Task InStatus<TStatusRef, TOrganizationRef>(TProject project);
}