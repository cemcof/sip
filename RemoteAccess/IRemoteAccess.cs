using sip.Organizations;
using sip.Userman;

namespace sip.RemoteAccess;

public interface IRemoteAccess
{
    Task<RemoteAccessStatus> GetRemoteAccessStatus(IOrganization organization);
    Task<RemoteAccessSession> OpenSession(RemoteAccessSessionRequest sessionRequest);
    Task<RemoteAccessSession> JoinSession(RemoteAccessSession session, RemoteAccessSessionRequest joinRequest);
    Task KillSession(RemoteAccessSession session);

    /// <summary>
    /// For given user, return sessions that given user is authorized for at the moment.
    /// </summary>
    /// <param name="organization"></param>
    /// <param name="forUser">The user to be authorized</param>
    /// <returns></returns>
    Task<IEnumerable<RemoteAccessSessionRequest>> GetAuthorizedSessionsForUser(IOrganization organization, IUserClientInfo forUser);
}