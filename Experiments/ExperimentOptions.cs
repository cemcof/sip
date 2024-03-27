using sip.Messaging.Email;
using sip.Experiments.Workflows;

namespace sip.Experiments;

public class ExperimentOptions
{
    public string DisplayName { get; set; } = null!;
    public string DisplayTheme { get; set; } = "default";

    public Dictionary<string, IConfigurationSection> Info { get; set; } = new();
    public Dictionary<string, DataLifecycleOptions> DataLifecycles { get; set; } = new();
    public Dictionary<string, Workflow> Workflows { get; set; } = new();

    public WorfklowHubOptions? Workflowhub { get; set; } = null;
}