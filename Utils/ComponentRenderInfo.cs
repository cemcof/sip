namespace sip.Utils;

public struct ComponentRenderInfo
{
    public ComponentRenderInfo(Type component, Dictionary<string, object?>? parameters = null)
    {
        Component = component;
        if (parameters is not null)
            Parameters = parameters;
    }

    public Type Component { get; }
    public Dictionary<string, object?> Parameters { get; } = new();
}