using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Authorization;
using sip.Organizations.Centers;
using sip.RemoteAccess;
using sip.Schedule;

// Authorization logic for using jobs for instruments and accessing instruments remotely

namespace sip.Experiments;

public class IntrumentJobsUseRequirement(IInstrument instrumentName, bool fullControl = false)
    : IAuthorizationRequirement
{
    public IInstrument Instrument { get; } = instrumentName;
    public bool FullControl { get; } = fullControl;
}

public class InstrumentRemoteDesktopRequirement(
        IOrganization organization,
        IInstrument? instrument = null,
        bool requireAdminAccess = false)
    : IAuthorizationRequirement
{
    public IOrganization Organization { get; } = organization;
    public IInstrument? Instrument { get; } = instrument;
    public bool RequireAdminAccess { get; } = requireAdminAccess;
}

public class InstrumentJobsUseHandler(
        InstrumentRemoteConnectAuthorizationHandler remoteConnectAuthorizationHandler,
        IOptionsMonitor<CenterNetworkOptions> centerNetworkOptions,
        ILogger<InstrumentJobsUseHandler> logger)
    : AuthorizationHandler<IntrumentJobsUseRequirement>
{
    // We use same feature implemented in other handler. Consider moving this somewhere else 
    // since this dependency does not make much sense

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        IntrumentJobsUseRequirement requirement)
    {
        // Lab operators can access any instrument at any time
        if (context.User.IsInRoleOfScope(nameof(LabOperatorRole), requirement.Instrument.Organization.Id))
        {
            context.Succeed(requirement);
        }

        // IP access is also possible
        var trustedProxies = centerNetworkOptions.Get(requirement.Instrument.Organization).TrustedProxies;
        if (context.User.CheckRemoteIp(requirement.Instrument.IPs, trustedProxies))
        {
            context.Succeed(requirement);
        }

        // If we are requiring full control, dont give a chance to ordinary lab user
        if (requirement.FullControl) return;
        if (context.User.IsInRoleOfScope(nameof(LabUserRole), requirement.Instrument.Organization.Id))
        {
            // However, lab users can only use if they have reservation at the moment
            try
            {
                // Find if we have any authorized sessions at the moment
                var session = (await remoteConnectAuthorizationHandler.GetAuthorizedSessionsForUser(
                        requirement.Instrument.Organization,
                        context.User.ToUserClientInfo()))
                    .FirstOrDefault(s => s.ForInstrument.Equals(requirement.Instrument));

                if (session is not null)
                {
                    context.Succeed(requirement);
                }
            }
            catch (NotAvailableException)
            {
                // Data not available - do not authorize
            }
        }
    }
}

public class InstrumentRemoteConnectAuthorizationHandler(
        IReservationsProvider reservationsConnector,
        ICenterProvider centerProvider,
        IOptionsMonitor<RemoteAccessOptions> remoteAccessOptions,
        ILogger<InstrumentRemoteConnectAuthorizationHandler> logger)
    : AuthorizationHandler<InstrumentRemoteDesktopRequirement>
{
    private readonly ILogger<InstrumentRemoteConnectAuthorizationHandler> _logger = logger;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        InstrumentRemoteDesktopRequirement requirement)
    {
        // Remote instrument admins can access any instrument at any time
        if (context.User.IsInRoleOfScope(nameof(RemoteMicAdminRole), requirement.Organization.Id))
        {
            context.Succeed(requirement);
        }
        else if (requirement.RequireAdminAccess)
        {
            // We are requiring admin access, but user is not remote access admin, so we have to reject
            return;
        }

        if (requirement.Instrument is null)
        {
            return;
        }

        if (context.User.IsInRoleOfScope(nameof(LabUserRole), requirement.Organization.Id))
        {
            // However, lab users can only use if they have reservation at the moment
            try
            {
                // Find if we have any authorized sessions at the moment
                var session = (await GetAuthorizedSessionsForUser(requirement.Instrument.Organization,
                        context.User.ToUserClientInfo()))
                    .FirstOrDefault(s => s.ForInstrument.Equals(requirement.Instrument));

                if (session is not null)
                {
                    context.Succeed(requirement);
                }
            }
            catch (NotAvailableException)
            {
                // Data not available - do not authorize
            }
        }
    }

    public async Task<IEnumerable<RemoteAccessSessionRequest>> GetAuthorizedSessionsForUser(
        IOrganization organization,
        IUserClientInfo forUser,
        DateTime moment = default)
    {
        if (moment == default) moment = DateTime.UtcNow;

        var scheduleData = await reservationsConnector.GetReservationsDataAsync(organization);
        var remoteAccOpts = remoteAccessOptions.Get(organization.Id);

        var result = new List<RemoteAccessSessionRequest>();

        // TODO - maybe refactor this into an actual comparer?
        bool UserNameComparer(IUserInfo user1, IUserInfo user2)
            => (!string.IsNullOrWhiteSpace(user1.Firstname) && // Firstname must exist and be equal
                !string.IsNullOrWhiteSpace(user2.Firstname) &&
                user1.Firstname.IsEqualIgnoreWsAndDiacritics(user2.Firstname))
               &&
               (!string.IsNullOrWhiteSpace(user1.Lastname) && // Lastname must exist and be equal
                !string.IsNullOrWhiteSpace(user2.Lastname) &&
                user1.Lastname.IsEqualIgnoreWsAndDiacritics(user2.Lastname));

        foreach (var inst in scheduleData.Instruments)
        {
            // Do we have this instrument as remote accessible?
            if (!remoteAccOpts.Instruments.ContainsKey(inst.InstrumentSubject))
                continue;

            // Now find proper reservation if exists
            var reservation = inst.Reservations
                // Reservations for this user
                .Where(r => UserNameComparer(r.ForUser, forUser) || UserNameComparer(r.CreatedBy, forUser))
                // Reservations for this moment
                .Where(r => r.Since < moment && r.Until > moment)
                .MaxBy(r => r.Until);

            if (reservation is not null)
            {
                // Reservation found, create session requests for all remote connections of related instrument
                result.AddRange(
                    remoteAccOpts.Instruments[inst.InstrumentSubject]
                        .Select(rem => new RemoteAccessSessionRequest(
                            inst.InstrumentSubject,
                            rem,
                            forUser,
                            false,
                            reservation.Until +
                            TimeSpan.FromHours(
                                1) // We add a hour more in order to not immediatelly kick the user from his work
                        ))
                );
            }
        }

        return result;
    }
}