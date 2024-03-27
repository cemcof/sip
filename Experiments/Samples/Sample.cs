using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using sip.Experiments.Model;
using sip.Forms;
using sip.Utils;

namespace sip.Experiments.Samples;

public class Sample : IStringFilter, IEquatable<Sample>
{
    #region EQUALS_BOILERPLATE

        public bool Equals(Sample? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Sample) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(Sample? left, Sample? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Sample? left, Sample? right)
        {
            return !Equals(left, right);
        }
    #endregion

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