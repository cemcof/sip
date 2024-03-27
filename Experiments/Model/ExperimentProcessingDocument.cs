using System.Text.Json.Serialization;
using sip.Documents;

namespace sip.Experiments.Model;

public class ExperimentProcessingDocument : Document
{
    public Guid ExperimentProcessingId { get; set; }
    [JsonIgnore, YamlIgnore] public ExperimentProcessing ExperimentProcessing { get; set; } = null!;
}