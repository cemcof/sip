namespace sip.Organizations.Centers;

public class CenterOptions
{
    [Required] public string Id { get; set; } = null!;
    [Required] public string Key { get; set; } = null!;
    
    public string? ConfigFile { get; set; }
}