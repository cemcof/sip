namespace sip.Projects.Statuses;

public class StatusDefinition : INamedSetup<StatusOptions>
{
    public virtual string Name => GetType().Name;

    public virtual void Setup(StatusOptions opts)
    {
        opts.StatusDetails = new StatusInfo(Name) {DisplayName = Name.Humanize()}; 
    }
    
    public static string GetName<TStatus>() where TStatus : StatusDefinition => Activator.CreateInstance<TStatus>().Name;

    public class DefaultStatus : StatusDefinition;
}