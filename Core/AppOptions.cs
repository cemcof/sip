namespace sip.Core;

public class AppOptions
{
    [Required] public string Name { get; set; } = null!;
    [Required] public string ShortName { get; set; } = null!;
    public string? AttachmentsDirectory { get; set; }
    [Required] public Uri UrlBase { get; set; } = null!;
    [Required] public Uri UrlBaseLocal { get; set; } = null!;

    public string? UserSupportMail { get; set; }
    [Required] public string AdminSupportMail { get; set; } = null!;

    [Required] public Assembly HostAppAssembly { get; set; } = Assembly.GetExecutingAssembly();

    [Required]
    public string Identifier => HostAppAssembly.GetName().Name!;

    public Type MasterComponent { get; set; } = typeof(App);

    [Required] public string DataDirectory { get; set; } = null!;
}