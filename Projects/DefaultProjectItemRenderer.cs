namespace sip.Projects;

public class DefaultProjectItemRenderProvider<TProject> : IProjectItemRenderProvider<TProject> where TProject : Project
{
    private readonly IProjectDefine<TProject> _projectDefine;

    public DefaultProjectItemRenderProvider(IProjectDefine<TProject> projectDefine)
    {
        _projectDefine = projectDefine;
    }
    
    public ComponentRenderInfo GetProjectItemComponent(TProject project)
    {
        return new ComponentRenderInfo(
            typeof(DefaultProjectItem),
            new Dictionary<string, object?>
            {
                [nameof(DefaultProjectItem.Badge)] = _projectDefine.DisplayName,
                [nameof(DefaultProjectItem.Theme)] = _projectDefine.Theme,
                [nameof(DefaultProjectItem.Note)] = "This is -> TODO delme"
            }
        );
    }

    public Type GetProjectComponent()
    {
        throw new NotImplementedException();
    }
}