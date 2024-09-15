// Model file, disable unwanted inspections:
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global, 'Jiri Novacek', '2019-06-12 05:43:13', '2019-06-12 05:43:13', '2019-06-12 05:43:13', '2019-06-12 05:43:13', '1S118', 'Resolved', '2019-12-09 07:58:38', 'high temperature and humidity in the laboratory.\n190611 - P. Mokros contacted to regulate the cooling unit\n190619 - the unit can only cool down the air, but the consequent heating is disabled due to the fact that heating water is off in Summer at the whole campus, system switched back to auto mode, will re-visit the issue after P. Saranchuk is back from vacation (190624).\n- no response from SUKB for several month.', 'The issue will be solved during facility reconstruction.'),

namespace sip.LabIssues;

public enum IssueStatus
{
    Initiated,
    InProgress,
    Resolved
}

public enum IssueUrgency
{
    Auto,
    Low,
    Medium,
    High
}

public class Issue : IStringFilter
{
    [StringLength(10)]
    public string Id { get; set; } = null!;
    
    public Guid? InitiatedById { get; set; }
    public AppUser? InitiatedBy { get; set; }

    public Guid? ResponsibleId { get; set; }
    public AppUser? Responsible { get; set; }

    public DateTime DtObserved { get; set; }
    public DateTime DtCreated { get; set; }
    public DateTime DtLastChange { get; set; }
    public DateTime DtAssigned { get; set; }

    [Required, Render(NoteIn = "Equipment/Instrument/Room")] 
    public string Subject { get; set; } = string.Empty;
    [Required, Render(NoteIn = "Describe what is the problem, why it occured and how to possibly solve it")] 
    public string Description { get; set; } = string.Empty;
    public string SolutionDescription { get; set; } = string.Empty;

    public IssueStatus Status { get; set; }
    public IssueUrgency Urgency { get; set; }

    public DateTime DtLastNotified { get; set; }
    
    [Render(Title = "Notify every", Unit = "days")]
    [Range(0.0, 365.0)]
    public double NotifyIntervalDays { get; set; } = 7;

    public bool IsFilterMatch(string? filter = null)
    {
        return StringUtils.IsFilterMatchAtLeastOneOf(filter, 
            Subject, 
            Status.ToString(),
            Urgency.ToString(), 
            Description, 
            SolutionDescription, 
            InitiatedBy?.Fullcontact ?? "",
            Responsible?.Fullcontact ?? "",
            DtObserved.StandardFormat()
            );
    }
}