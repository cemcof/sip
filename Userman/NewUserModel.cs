namespace sip.Userman;

public class NewUserModel
{
    public AppUser UserDetails { get; set; } = AppUser.NewEmpty();

    /// <summary>
    /// Whether the user should be invited to the system via external login
    /// </summary>
    public bool InviteToTheSystem { get; set; }

    /// <summary>
    /// Orcid of the user, if given, it is directly added to the user as a login provider
    /// </summary>
    public string Orcid { get; set; } = null!;
}