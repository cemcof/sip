using sip.Userman;

namespace sip.Auth;

public interface IAppUserProvider
{
    Task<AppUser?> FindByCpAsync(ClaimsPrincipal claimsPrincipal, bool loadLogins = false, CancellationToken cancellationToken = default);
}