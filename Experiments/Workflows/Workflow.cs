using sip.Forms.Dynamic;

namespace sip.Experiments.Workflows;

public class Workflow : IIdentified<string> {
    public string Id { get; set; } = null!;
    public string Title { get; set; } = "<untitled workflow>";
    public string Description { get; set; } = "<no description>";

    public string? Diagram { get; set; }
    public string? Provider { get; set; }

    public List<string> Tags { get; set; } = new();

    public object? Data { get; set; } = new Dictionary<string, object>();
    
    // Only for automatic configuration binding
    public IConfigurationSection DataConf
    {
        // TODO - maybe we must have getter otherwise binder will ignore this
        // get
        // {
        //     if (Data is null) return new ConfigurationRoot(ArraySegment<IConfigurationProvider>.Empty);
        //     throw new NotSupportedException();
        // };
        get => null!;
        set => Data = value.ToObject();
    }

}

