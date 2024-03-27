using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace sip.Experiments.Model;

public class ExperimentProcessing
{
    public Guid Id { get; set; }

    public Guid ExperimentId { get; set; }
    [JsonIgnore, YamlIgnore] public Experiment Experiment { get; set; } = null!;

    [MaxLength(64)] public string ProcessingEngine { get; set; } = string.Empty;
    [MaxLength(64)] public string Node { get; set; } = string.Empty;
    [MaxLength(16)] public string? Pid { get; set; } 
    
    [NotMapped] public List<Dictionary<string, object?>> Workflow { get; set; } = new();
    [YamlIgnore, JsonIgnore] public string WorkflowSerialized { get; set; } = "[]";

    [YamlIgnore, JsonIgnore] public List<ExperimentProcessingDocument> ExperimentProcessingDocuments { get; set; } = new();

    [YamlIgnore, JsonIgnore, NotMapped] public ExperimentProcessingDocument ResultReport =>
        ExperimentProcessingDocuments.Single(d => d.Name == nameof(ResultReport));
    public Guid ResultDocumentId => ResultReport.Id; // For json output, cannot serialize document for some reason
    
    [YamlIgnore, JsonIgnore, NotMapped] public ExperimentProcessingDocument LogReport =>
        ExperimentProcessingDocuments.Single(d => d.Name == nameof(LogReport));
    public Guid LogDocumentId => LogReport.Id; // For json output, cannot serialize document for some reason
    
    public void DeserializeWorkflow()
    {
        var ds = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .WithAttemptingUnquotedStringTypeDeserialization()
            .Build();
        Workflow = ds.Deserialize<List<Dictionary<string, object?>>>(WorkflowSerialized);
    }

    public void SerializeWorkflow()
    {
        var ser = new SerializerBuilder()
            .JsonCompatible()
            .Build();
        WorkflowSerialized = ser.Serialize(Workflow);
    }
    public ProcessingState State { get; set; }
}

public enum ProcessingState
{
    Ready,
    Running,
    Completed,
}