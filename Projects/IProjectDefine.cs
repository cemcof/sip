namespace sip.Projects;

public interface IProjectDefine
{
    Type ProjectType { get; }
    string DisplayName { get; }
    string Theme { get; }
}

public interface IProjectDefine<TProject> : IProjectDefine
{
    
}

