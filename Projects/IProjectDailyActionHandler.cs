namespace sip.Projects;

public interface IProjectDailyActionHandler<in TProject> where TProject : Project
{
    Task HandleDailyActionAsync(TProject project);
}