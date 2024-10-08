using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using sip.Experiments.Logs;
using sip.Experiments.Samples;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace sip.Experiments.Model;

public class Experiment : IIdentified<Guid>
{
    // Identification, names and types
    public Guid Id { get; set; }

    [MaxLength(128)] public string SecondaryId { get; set; } = null!;
    
    // Organization 
    public Organization Organization { get; set; } = null!;
    public string OrganizationId { get; set; } = null!;

    // Operator  
    [YamlIgnore, JsonIgnore] public Guid OperatorId { get; set; }
    [Required] public AppUser Operator { get; set; } = null!;
        
    // User
    [YamlIgnore, JsonIgnore] public Guid UserId { get; set; }
    [Required] public AppUser User { get; set; } = null!;
    [MaxLength(128), Required] public string UserType { get; set; } = null!;
    
    // Organization user
    public string? OrganizationUserId { get; set; }
    public Organization? OrganizationUser { get; set; }
        
    // Project
    public string? ProjectId { get; set; }
    [NotMapped] public string? ProjectAcronym => Project?.Acronym; // Temporary solution for serialization - preamble random error
    [NotMapped] public string? ProjectName => Project?.Title; // Temporary solution for serialization - preamble random error
    [YamlIgnore, JsonIgnore] public Project? Project { get; set; }
        
    // Sample 
    [YamlIgnore, JsonIgnore] public Guid SampleId { get; set; }
    [Required] public Sample Sample { get; set; } = null!;
    
    // Logs
    [YamlIgnore, JsonIgnore] public List<Log> Logs { get; set; } = new();

    [MaxLength(64), Required] public string Technique { get; set; } = null!;
    [MaxLength(64), Required] public string InstrumentName { get; set; } = null!;

    [NotMapped] public string Type => $"{InstrumentName}/{Technique}";

    // Experiment status
    public DateTime DtCreated { get; set; }
    public DateTime DtStateChanged { get; set; }
    public ExpState State { get; set; }
    
    public ExperimentStorage Storage { get; set; } = null!;
    public ExperimentProcessing Processing { get; set; } = null!;
    public ExperimentPublication Publication { get; set; } = null!;
    
    public ExperimentDataSource DataSource { get; set; } = null!;
    
    
    
    [NotMapped, YamlIgnore, JsonIgnore] public (string center, string instrument, string technique) KeyIdentif => (OrganizationId, InstrumentName, Technique);
    [NotMapped, YamlIgnore, JsonIgnore] public string KeyIdentifStr => $"{OrganizationId}/{InstrumentName}/{Technique}";
    
    // Notes for the user        
    [Render(Title = "Notes", Tip = "Notes will be inserted to the notification email sent to the user after experiment is finished")]
    public string? Notes { get; set; } = string.Empty;
    
    [Render(Tip = "Send email notification to the user with information how to access the data.", 
        Title = "Send user link to this experiment")]
    public bool NotifyUser { get; set; } = true;

    
}

public enum ExpState
{
    Idle, 
    StartRequested,
    Active,
    StopRequested,
    Finished
}
