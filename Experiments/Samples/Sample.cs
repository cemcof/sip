using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using sip.Experiments.Model;

namespace sip.Experiments.Samples;

public class Sample : IStringFilter, IIdentified<Guid>
{
    public Guid Id { get; set; }
        
    [Required] public string Name { get; set; } = string.Empty;

    public string KeywordsStr { get; set; } = string.Empty;
        
    [Required]
    [NotMapped]
    [MinLength(1)]
    [Render(Tip = "Set of keywords which will allow to more precise classification of the sample (e.g. RNA polymerase; Escherichia coli; transcription initiation complex; RNA binding protein).")]
    public List<string> Keywords { 
        get => KeywordsStr.Split("; ")
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList(); 
        set => KeywordsStr = string.Join("; ", value); 
    }

    [Render(Tip =
        "Sample identifier in public repository (e.g. sample DOI, DOI in relevant publication, entry from BioSamples etc.")]
    public string? Identifier { get; set; } 

    [Render(Tip =
        "File with the project proposal relevant to the measured sample (e.g. project proposal file from Aria or CIISB application)")]
    public string? File { get; set; }

    [JsonIgnore] [YamlIgnore] public List<Experiment> InExperiments { get; set; } = new();
        
    public bool IsFilterMatch(string filter = "")
    {
        return StringUtils.IsFilterMatchAtLeastOneOf(filter, Name, KeywordsStr, Identifier);
    }
}