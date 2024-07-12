using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Internal;
using sip.Scheduling;
using sip.Utils;

namespace sip.Organizations.Centers;

public class CenterManager(
        IOptionsMonitor<CentersOptions> centersOptions,
        IOptionsMonitor<ScheduledServiceOptions> schedOpts,
        TimeProvider timeProvider,
        ILogger<CenterManager> logger,
        CenterConfigurationProvider centerConfigurationProvider,
        IOrganizationProvider organizationProvider)
    : ScheduledService(schedOpts, timeProvider, logger),
        ICenterProvider,
        INodeStatusProvider
{

    // In-memory storage of active centers keyed to their organizations
    private readonly ConcurrentDictionary<IOrganization, CenterStatus> _centers = new();

    private const string SELF_NODE_NAME = "Webserver";

    private async Task _RefreshSelfCentersAsyncHelper()
    {
        JsonElement DeserializeConfigurationFromFile(string configFile)
        {
            // We need to get it from yaml to JsonElement
            using var reader = new StreamReader(configFile);
            var yamlParser = new MergingParser(new Parser(reader));
            var yamlDeserializer = new DeserializerBuilder()
                .WithAttemptingUnquotedStringTypeDeserialization()
                .Build();
            var configDeser = yamlDeserializer.Deserialize(yamlParser);
            return JsonSerializer.SerializeToElement(configDeser);
        }

        // Ping all centers that are configured from this webserver
        foreach (var centerOptions in centersOptions.CurrentValue.Centers.Where(c => !string.IsNullOrEmpty(c.ConfigFile)))
        {
            var org = organizationProvider.GetFromString(centerOptions.Id);

            CenterStatus AddValueFactory(IOrganization organization)
            {
                var deserializedConfig = DeserializeConfigurationFromFile(centerOptions.ConfigFile!);
                centerConfigurationProvider.LoadCenter(organization.Id, deserializedConfig);
                return new CenterStatus(org, SELF_NODE_NAME, deserializedConfig)
                {
                    LastChange = timeProvider.DtUtcNow(), LastPing = timeProvider.DtUtcNow()
                };
            }
            
            CenterStatus UpdateValueFactory(IOrganization organization, CenterStatus centerStatus)
            {
                // Deserialize configuration only if newer
                if (File.GetLastWriteTimeUtc(centerOptions.ConfigFile!) > centerStatus.LastChange)
                {
                    centerStatus.Configuration = DeserializeConfigurationFromFile(centerOptions.ConfigFile!);
                    centerConfigurationProvider.LoadCenter(organization.Id, centerStatus.Configuration);
                    centerStatus.LastChange = timeProvider.DtUtcNow();
                }

                centerStatus.LastPing = timeProvider.DtUtcNow();
                centerStatus.SubmittedByNode = SELF_NODE_NAME;
                return centerStatus;
            }

            _centers.AddOrUpdate(org, AddValueFactory, UpdateValueFactory);
        }

    }

    private void _KillInactiveCentersHelper()
    {
        // Kill/remove center that are inactive for too long
        var killAfter = centersOptions.CurrentValue.KillAfterInactive;
        foreach (var centerDead in _centers
                     .Where(c => timeProvider.DtUtcNow() - c.Value.LastPing > killAfter)
                     .ToList())
        {
            _centers.Remove(centerDead.Key, out _);
        }
    }
    
    
    protected override async Task ExecuteRoundAsync(CancellationToken stoppingToken)
    {
        await _RefreshSelfCentersAsyncHelper();
        _KillInactiveCentersHelper();
    }

    public Task SubmitCenterConfigurationAsync(string submitterNode, string organizationId,
        JsonElement configuration, CancellationToken ct = default)
    {
        // Get organization object by ID
        var organization = organizationProvider.GetFromString(organizationId);
        
        var currCenter = _centers.AddOrUpdate(organization, 
            _ => new CenterStatus(organization, submitterNode, configuration),
            (_, centerStatus) =>
            {
                centerStatus.Configuration = configuration;
                centerStatus.SubmittedByNode = submitterNode;
                return centerStatus;
            });
        
        // Adjust center timing info
        currCenter.LastChange = timeProvider.DtUtcNow();
        currCenter.LastPing = timeProvider.DtUtcNow();
        
        // Refresh options so the fetch new configuration for this center
        centerConfigurationProvider.LoadCenter(organizationId, configuration);
        return Task.CompletedTask;
    }

    
    public IEnumerable<CenterStatus> GetAvailableCenters() => _centers.Values.ToList();

    public Task<CenterStatus> GetStatusFromStringAsync(string organizationStr)
    {
        var org = organizationProvider.GetFromString(organizationStr);
        var result =  _centers.GetValueOrDefault(org) 
               ?? throw new NotAvailableException($"Organization {organizationStr} is not available");

        return Task.FromResult(result);
    }

    public Task<CenterStatus> GetFromKeyAsync(string organizationKey)
    {
        var orgId = centersOptions.CurrentValue.Centers.FirstOrDefault(c => c.Key == organizationKey)?.Id;
        if (string.IsNullOrEmpty(orgId))
        {
            throw new NotAvailableException($"Organization referenced by given secret key is not available");
        }

        return GetStatusFromStringAsync(orgId);
    }

    public string KeyToOrgName(string organizationKey)
    {
        var orgId = centersOptions.CurrentValue.Centers.FirstOrDefault(c => c.Key == organizationKey)?.Id;
        if (string.IsNullOrEmpty(orgId)) throw new NotAvailableException();
        return orgId;
    }

    public Task<Dictionary<string, CenterNodeStatus>> GetNodesStatusAsync(IOrganization organizationSubject)
    {
        if (_centers.TryGetValue(organizationSubject, out var centerStatus)) 
        {
            return Task.FromResult(centerStatus.Nodes);    
        }

        return Task.FromResult(new Dictionary<string, CenterNodeStatus>());
    }
}
