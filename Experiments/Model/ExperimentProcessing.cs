using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace sip.Experiments.Model;

public class ExperimentProcessing
{
    public Guid Id { get; set; }

    public Guid ExperimentId { get; set; }
    [JsonIgnore, YamlIgnore] public Experiment Experiment { get; set; } = null!;

    [MaxLength(64)] public string ProcessingEngine { get; set; } = string.Empty;
    
    public string? WorkflowRef { get; set; }
    [NotMapped] public object? Workflow { get; set; } 
    [YamlIgnore, JsonIgnore] public string WorkflowSerialized { get; set; } = "[]";

    [YamlIgnore, JsonIgnore] public List<ExperimentProcessingDocument> ExperimentProcessingDocuments { get; set; } = new();

    [YamlIgnore, NotMapped] public ExperimentProcessingDocument ResultReport =>
        ExperimentProcessingDocuments.Single(d => d.Name == nameof(ResultReport));
    
    [YamlIgnore, NotMapped] public ExperimentProcessingDocument LogReport =>
        ExperimentProcessingDocuments.Single(d => d.Name == nameof(LogReport));
    
    public void DeserializeWorkflow()
    {
        var ds = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .WithAttemptingUnquotedStringTypeDeserialization()
            .Build();
        Workflow = ds.Deserialize(WorkflowSerialized);
    }

    public void SerializeWorkflow()
    {
        var ser = new SerializerBuilder()
            .JsonCompatible()
            .Build();
        WorkflowSerialized = ser.Serialize(Workflow);
    }
    public ProcessingState State { get; set; }
    [MaxLength(64)] public string? Node { get; set; }
    [MaxLength(16)] public string? Pid { get; set; }

    public DateTime DtLastUpdate { get; set; }

    // For workflows filtering purposes
    [NotMapped, JsonIgnore, YamlIgnore] public List<string> WorkflowTags { get; set; } = [];
}

public enum ProcessingState
{
    Uninitialized,
    Ready,
    Running,
    StopRequested,
    Finalizing,
    Completed,
    Disabled
}