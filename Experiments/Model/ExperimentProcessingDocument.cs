using System.Text.Json.Serialization;

namespace sip.Experiments.Model;

public class ExperimentProcessingDocument : Document
{
    public Guid ExperimentProcessingId { get; set; }
    [JsonIgnore, YamlIgnore] public ExperimentProcessing ExperimentProcessing { get; set; } = null!;
}