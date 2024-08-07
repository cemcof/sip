namespace sip.Organizations;

public record OrganizationComponentContext(
    IOrganization? Organization,
    ClaimsPrincipal PrincipalUser,
    AppUser? ApplicationUser,
    Func<string, string>? UrlProvider
    );