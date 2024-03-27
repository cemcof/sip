namespace sip.Projects;

public class ProjectTypeInfo(Type projectType, string displayName, Type filterType)
{
    public Type   ProjectType { get; set; } = projectType;
    public Type   FilterType  { get; set; } = filterType;
    public string DisplayName { get; set; } = displayName;
}
