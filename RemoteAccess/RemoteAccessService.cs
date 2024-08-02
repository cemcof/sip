using Microsoft.Extensions.Internal;
using sip.Experiments;
using sip.Organizations;
using sip.Scheduling;
using sip.Userman;

namespace sip.RemoteAccess;

public record RemoteAccessSessionRequest(
    IInstrument ForInstrument,
    RemoteConnectionInfo ForConnection,
    IUserClientInfo ForUser,
    bool IsReadonly,
    DateTime Expiration
);

public record RemoteAccessStatus
{
    public List<RemoteAccessSession> ActiveSessions { get; }
    public Dictionary<IInstrument, List<RemoteConnectionInfo>> AvailableConnections { get; }

    public RemoteAccessStatus(
        HashSet<RemoteAccessSession> sessions, 
        Dictionary<IInstrument, List<RemoteConnectionInfo>> availableConnections)
    {
        AvailableConnections = availableConnections;
        ActiveSessions = sessions.ToList();
    }

    public IEnumerable<RemoteAccessSession> GetActiveSessions(IInstrument forInstrument,
        RemoteConnectionInfo forConnection) =>
        ActiveSessions.Where(a => a.ForConnection == forConnection && a.ForInstrument == forInstrument);

    // public Dictionary<IInstrumentSubject, Dictionary<RemoteConnectionInfo, List<RemoteAccessSession>>> ByInstruments
    // {
    //     get
    //     {
    //         // TODO - refactor
    //         // For now this is done kinda stupidly. LINQ should be probably more elegant solution
    //         
    //         var result =
    //             new Dictionary<IInstrumentSubject, Dictionary<RemoteConnectionInfo, List<RemoteAccessSession>>>();
    //         
    //         foreach (var s in _sessions)
    //         {
    //             if (!result.ContainsKey(s.ForInstrument))
    //                 result[s.ForInstrument] =
    //                     new Dictionary<RemoteConnectionInfo, List<RemoteAccessSession>>();
    //
    //
    //             if (!result[s.ForInstrument].ContainsKey(s.ForConnection))
    //                 result[s.ForInstrument][s.ForConnection] =
    //                     new List<RemoteAccessSession>();
    //             
    //             result[s.ForInstrument][s.ForConnection].Add(s);
    //         }
    //
    //         return result;
    //     }
    // }
    
    
}


public class RemoteAccessService(
        IOptionsMonitor<RemoteAccessOptions>              options,
        GuacamoleDriver                             guacamoleDriver,
        IOptionsMonitor<GuacamoleOptions>                 gcOptions,
        TimeProvider                                timeProvider,
        IOptionsMonitor<ScheduledServiceOptions>    schedOpts,
        ILogger<RemoteAccessService>                logger,
        InstrumentRemoteConnectAuthorizationHandler authorizationAuthorizationHandler)
    : ScheduledService(schedOpts, timeProvider, logger), IRemoteAccess
{
    private readonly HashSet<RemoteAccessSession> _sessions = new();


    public Task<RemoteAccessStatus> GetRemoteAccessStatus(IOrganization organization)
    {
        var rmOpts = options.Get(organization);

        return Task.FromResult(new RemoteAccessStatus(_sessions, rmOpts.Instruments));
    }

    public async Task<RemoteAccessSession> OpenSession(RemoteAccessSessionRequest sessionRequest)
    {
        var gcOpts = gcOptions.Get(sessionRequest.ForInstrument.Organization.Id);
        
        await KillReqClientSessions(sessionRequest);
        
        var guacaModel = GuacamoleDriver.GenerateSingleConnectionAuthModel(GenerateSessionName(sessionRequest),
            sessionRequest.Expiration,
            sessionRequest.ForInstrument.Name + "/" + sessionRequest.ForConnection.Name,
            sessionRequest.ForConnection.Protocol, 
            sessionRequest.ForConnection.Hostname, 
            sessionRequest.ForConnection.Port, 
            sessionRequest.ForConnection.Password);
        
        var token = guacamoleDriver.GenerateGuacaToken(guacaModel, gcOpts.SecretKey);
        var authToken = await guacamoleDriver.SessionConnect(gcOpts.BaseUrl, token);
        var url = guacamoleDriver.GenerateGuacaUrlFromToken(gcOpts.BaseUrl, authToken);

        var session = new RemoteAccessSession(
            sessionRequest.ForInstrument, 
            sessionRequest.ForConnection,
            sessionRequest.ForUser,
            DateTime.UtcNow,
            sessionRequest.Expiration,
            guacaModel.connections.First().Value.id,
            token,
            authToken,
            url
        );

        _sessions.Add(session);
        Logger.LogInformation("Added guacamole session: {@Session}", session);

        return session;
    }

    private string GenerateSessionName(RemoteAccessSessionRequest sessionRequest) =>
        sessionRequest.ForUser.Firstname + " " + sessionRequest.ForUser.Lastname + "/" + sessionRequest.ForUser.IpAddress +
        "/" + timeProvider.DtUtcNow().StandardFormat();

    public async Task<RemoteAccessSession> JoinSession(RemoteAccessSession session, RemoteAccessSessionRequest joinRequest)
    {
        var gcOpts = gcOptions.Get(session.ForInstrument.Organization.Id);
        
        var guacaModel = GuacamoleDriver.GenerateJoinAuthModel(
            session.SessionId,
            GenerateSessionName(joinRequest),
            joinRequest.ForInstrument.Name + "/" + joinRequest.ForConnection.Name,
            joinRequest.Expiration, 
            joinRequest.IsReadonly);
        
        var token = guacamoleDriver.GenerateGuacaToken(guacaModel, gcOpts.SecretKey);
        var authToken = await guacamoleDriver.SessionConnect(gcOpts.BaseUrl, token);
        var url = guacamoleDriver.GenerateGuacaUrlFromToken(gcOpts.BaseUrl, authToken);

        var joinSession = new RemoteAccessSession(
            joinRequest.ForInstrument, 
            joinRequest.ForConnection,
            joinRequest.ForUser,
            DateTime.UtcNow,
            joinRequest.Expiration,
            guacaModel.connections.First().Value.id,
            token,
            authToken,
            url
        );

        _sessions.Add(joinSession);
        
        return joinSession;
    }

    public async Task KillSession(RemoteAccessSession sessionInfo)
    {
        var gcOpts = gcOptions.Get(sessionInfo.ForInstrument.Organization.Id);
        if (_sessions.TryGetValue(sessionInfo, out var session) && !string.IsNullOrEmpty(session.AuthToken))
        {
            await guacamoleDriver.RemoveSessionIfExists(gcOpts.BaseUrl, session.AuthToken);
            _sessions.Remove(session);
        }

    }

    /// <summary>
    /// Kill existing "same" sessions as would be opened by given request.
    /// Same session is when:
    /// - User is same
    /// - Host is same 
    /// </summary>
    /// <param name="req"></param>
    public async Task KillReqClientSessions(RemoteAccessSessionRequest req)
    {
        var toBeKilled = _sessions.Where(
                s => s.ForUser.Id == req.ForUser.Id &&
                     s.ForConnection.Hostname == req.ForConnection.Hostname)
            .ToArray();
        
        foreach (var killme in toBeKilled)
        {
            await KillSession(killme);
        }
    } 

    public Task<IEnumerable<RemoteAccessSessionRequest>> GetAuthorizedSessionsForUser(IOrganization organization,
        IUserClientInfo forUser)
        => authorizationAuthorizationHandler.GetAuthorizedSessionsForUser(organization, forUser);

    protected override async Task ExecuteRoundAsync(CancellationToken stoppingToken)
    {
        // Check for expired sessions and kill them
        var toBeDeleted = _sessions.Where(s => s.Until < timeProvider.DtUtcNow())
            .ToHashSet();
        
        foreach (var delme in toBeDeleted)
        {
            if (!string.IsNullOrEmpty(delme.Token))
            {
                try
                {
                    await KillSession(delme);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Error during session deletion");
                    throw;
                }
            }
        }
        
        _sessions.ExceptWith(toBeDeleted);
    }
}