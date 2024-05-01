using System.Text.Json.Serialization;

namespace sip.Experiments.Model;

public class ExperimentPublication
{
    public Guid Id { get; set; }

    [MaxLength(64)] public string PublicationEngine { get; set; } = string.Empty;
    public Guid ExperimentId { get; set; }
    [YamlIgnore, JsonIgnore] public Experiment Experiment { get; set; } = null!;
    
    public string? Doi { get; set; }
    public string? RecordId { get; set; }
    public string? TargetUrl { get; set; }
    
    /// <summary>
    /// After how much time after experiment start the publication should be requested
    /// </summary>
    public TimeSpan EmbargoPeriod { get; set; }
    /// <summary>
    /// The actual datetime of the embargo release and publication request
    /// </summary>
    public DateTime DtEmbargo { get; set; }

    public PublicationState State { get; set; }
    [MaxLength(64)] public string? Node { get; set; }

}

public enum PublicationState
{
    None,
    Unpublished,
    DraftCreationRequested,
    DraftCreated,
    PublicationRequested,
    Published
}