namespace sip.Utils.Crm;

public class CrmOptions
{
    [Required] public string Username { get; set; } = null!;
    [Required] public string Password { get; set; } = null!;
    [Required] public Uri BaseUrl { get; set; } = null!;
}