namespace sip.Projects.Statuses;

public class StatusOptions
{
    [Required] public StatusInfo StatusDetails { get; set; } = null!;
}