namespace sip.Messaging;

public interface IProjectMessageConsumer<TProject> where TProject : Project
{
    Task ConsumeMessageAsync(ProjectOrganizationMessage message);
}