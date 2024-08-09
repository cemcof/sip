namespace sip.Projects;

public interface IProjectMessaging
{
    Task<IEnumerable<MessageRecipient>> GetResendRecipientsAsync(ProjectOrganizationMessage message);
}

public interface IProjectMessaging<TProject> where TProject : Project
{
    Task<IEnumerable<MessageRecipient>> GetResendRecipientsAsync(ProjectOrganizationMessage message);

}


public class ProjectMessagingDelegatorService(IServiceProvider serviceProvider) : IProjectMessaging
{
    public Task<IEnumerable<MessageRecipient>> GetResendRecipientsAsync(ProjectOrganizationMessage message)
    {
        dynamic mess = serviceProvider.GetRequiredService(typeof(IProjectMessaging).MakeGenericType(message.Project.GetType()));
        return mess.GetResendRecipientsAsync(message);
    }
}