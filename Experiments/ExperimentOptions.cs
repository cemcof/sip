using sip.Experiments.Workflows;

namespace sip.Experiments;

public class ExperimentOptions
{
    public string DisplayName { get; set; } = null!;
    public string DisplayTheme { get; set; } = "default";
    public int DisplayOrder { get; set; } = int.MaxValue;

    public Dictionary<string, DataLifecycleOptions> DataLifecycles { get; set; } = new();
    public Dictionary<string, Workflow> Workflows { get; set; } = new();

    public TimeSpan IdleTimeout { get; set; } = TimeSpan.FromHours(3);
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