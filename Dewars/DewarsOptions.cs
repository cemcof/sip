namespace sip.Dewars;

public class DewarOptions
{
    [Required] public string Name { get; set; } = null!;
    public int HoldersCount { get; set; } = 6;
}

public class DewarsOptions
{
    public ICollection<DewarOptions> DewarList { get; set; } = new List<DewarOptions>();
}