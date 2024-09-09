using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace sip.Experiments.Model;

public class ExperimentDataSource
{
    public Guid Id { get; set; }
    
    public Guid ExperimentId { get; set; }
    [YamlIgnore, JsonIgnore] public Experiment Experiment { get; set; } = null!;
    
    public string? SourceDirectory { get; set; }
    public string? SourcePatternsStr { get; set; }
    [NotMapped]
    public List<string> SourcePatterns { 
        get => (SourcePatternsStr ?? string.Empty)
            .Split(";", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList(); 
        set => SourcePatternsStr = string.Join(";", value); 
    }
    
    public bool KeepSourceFiles { get; set; } = false;
    public TimeSpan? CleanAfter { get; set; }
    public DateTime? DtCleaned { get; set; }
}