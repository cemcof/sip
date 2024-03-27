using sip.Experiments;
using sip.Userman;

namespace sip.RemoteAccess;
public record RemoteAccessSession(
    IInstrument ForInstrument,
    RemoteConnectionInfo ForConnection, 
    IUserClientInfo ForUser,
    DateTime From,
    DateTime Until,
    
    string SessionId,
    string Token,
    string AuthToken,
    string TargetUrl
);