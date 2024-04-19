using sip.Messaging.Email;
using sip.Experiments.Workflows;
using sip.Forms.Dynamic;

namespace sip.Experiments;

public class ExperimentOptions
{
    public string DisplayName { get; set; } = null!;
    public string DisplayTheme { get; set; } = "default";


    public Dictionary<string, DataLifecycleOptions> DataLifecycles { get; set; } = new();
    public Dictionary<string, Workflow> Workflows { get; set; } = new();

    public object? DynInfo { get; set; }
    
    // For configuration binding purposes
    public IConfigurationSection Info
    {
        get => null!;
        set => DynInfo = value.ToObject();    
    }
    
}

public class EngineOptions
{
    [Required] public string Id { get; set; } = null!; 
    public string? Name { get; set; } 
    public string? Description { get; set; }
}

public class EnginesOptions
{
    public List<EngineOptions> Processing { get; set; } = new();
}