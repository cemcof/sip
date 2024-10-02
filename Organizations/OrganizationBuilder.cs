using Microsoft.EntityFrameworkCore.Metadata.Builders;
using sip.Core;
using sip.Organizations.Centers;

namespace sip.Organizations;

public class OrganizationBuilder
{
    private readonly IServiceCollection _services;
    private readonly ILogger<OrganizationBuilder> _logger;
    private readonly ConfigurationManager _configurationManager;

    public OrganizationBuilder(IServiceCollection services, ILogger<OrganizationBuilder> logger, ConfigurationManager configurationManager)
    {
        _services = services;
        _logger = logger;
        _configurationManager = configurationManager;

        AddOrganizationsFromConfiguration();

        // Organizations seeding (from named options)
        _services.AddOptions<DbSeedOptions>()
            .Configure<IOptionListProvider<OrganizationOptions>>((seed, orgopts) =>
            {
                var organizations = orgopts.GetAll().Select(x => x.OrganizationDetails);
                seed.SeedEntities.AddRange(organizations);
            });

        _services.AddSingleton<OrganizationService>();
        _services.AddSingleton<IOrganizationProvider>(s => s.GetRequiredService<OrganizationService>());

        // Configure gui per-organization
        _services.AddOptions<MainGuiOptions>()
            .GetOrganizationOptionsBuilder(configurationManager)
            .BindOrganizationConfiguration("Gui");

        _services.AddScoped<OrganizationActionFilter>();


    }

    public OrganizationBuilder AddOrganizationType<TOrg>(Action<EntityTypeBuilder<TOrg>>? modelConf = null)
         where TOrg : Organization
    {
        modelConf ??= x => { x.Property(o => o.Id); };
        _services.ConfigureDbModel(mb => modelConf(mb.Entity<TOrg>()));
        return this;
    }

    public OrganizationBuilder AddOrganization<TOrganizationRef>() where TOrganizationRef : OrganizationDefinition
    {
        _services.ConfigureFromNamedSetup<OrganizationOptions, TOrganizationRef>();
        return this;
    }
    
    /// <summary>
    /// List organizations section from configuration and extract organizations from it, configure them.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public OrganizationBuilder AddOrganizationsFromConfiguration()
    {
        var orgs = _configurationManager.GetSection("Organizations");
        foreach (var org in orgs.GetChildren())
        {
            var type = org.GetValue<string>("Type");

            var id = org.GetValue<string>(nameof(Organization.Id)) ?? 
                     throw new ArgumentNullException(nameof(Organization.Id));
            
            var linkId = org.GetValue<string>(nameof(Organization.LinkId)) ?? 
                         throw new ArgumentNullException(nameof(Organization.LinkId));
            
            var name = org.GetValue<string>(nameof(Organization.Name)) ?? 
                       throw new ArgumentNullException(nameof(Organization.Name));
            
            var abbreviation = org.GetValue<string>(nameof(Organization.Abbreviation)) ?? 
                              throw new ArgumentNullException(nameof(Organization.Abbreviation));
            
            var displayName = org.GetValue<string?>(nameof(Organization.DisplayName));
            var description = org.GetValue<string?>(nameof(Organization.Description));
            var url = org.GetValue<string?>(nameof(Organization.Url));


            Organization organ = type switch
            {
                nameof(ResearchFacility) => new ResearchFacility(id, linkId, name, abbreviation)
                    {DisplayName = displayName, Description = description, Url = url},
                nameof(ResearchCenter) => new ResearchCenter(id, linkId, name, abbreviation)
                    {DisplayName = displayName, Description = description, Url = url},
                nameof(ResearchInfrastructure) => new ResearchInfrastructure(id, linkId, name, abbreviation)
                    {DisplayName = displayName, Description = description, Url = url},
                nameof(Company) => new Company(id, linkId, name, abbreviation)
                    {DisplayName = displayName, Description = description, Url = url},
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown or unsupported organization type")
            };

            _services.Configure<OrganizationOptions>(organ.Id, o =>
            {
                o.OrganizationDetails = organ;
            });
        }
        return this;
    }

    public OrganizationBuilder ScanForOrganizations(IEnumerable<Assembly> assemblies)
    {
        foreach (var assembly in assemblies)
        {
            ScanForOrganizations(assembly);
        }

        return this;
    }

    public OrganizationBuilder ScanForOrganizations(Assembly assembly)
    {
        // Firstly, discover and add organization types
        _logger.LogDebug("Scanning for organizations types in assembly {}", assembly.FullName);
        var orgTypeAdder = GetType().GetMethod(nameof(AddOrganizationType))!;
        var orgTypes = assembly.GetTypes()
            .Where(t => typeof(Organization).IsAssignableFrom(t));
        
        foreach (var orgType in orgTypes)
        {
            _logger.LogDebug("Discovered organization type: {}", orgType.Name);
            var genadder = orgTypeAdder.MakeGenericMethod(orgType);
            genadder.Invoke(this, new object?[] { null });
        }
        
        // Secondly, discover and add particular organizations
        _logger.LogDebug("Scanning for organizations in assembly {}", assembly.FullName);
        var adder = GetType().GetMethod(nameof(AddOrganization))!;
        
        var refs = assembly.FindImplementations<OrganizationDefinition>(includeSelf:false);
        
        foreach (var type in refs)
        {
            _logger.LogDebug("Discovered organization ref: {}", type.Name);
            var genadder = adder.MakeGenericMethod(type);
            genadder.Invoke(this, Array.Empty<object?>());
        }

        return this;
    }

    public OrganizationBuilder AddCenters()
    {
        _services.AddSingleton<CenterManager>();
        _services.AddSingleton<ICenterProvider>(s => s.GetRequiredService<CenterManager>());
        _services.AddSingleton<INodeStatusProvider>(s => s.GetRequiredService<CenterManager>());

        _services.AddOptions<NodesOptions>()
            .GetOrganizationOptionsBuilder(_configurationManager)
            .Configure((options, configuration, _) =>
            {
                var nodes = configuration.GetSection("LimsNodes").GetChildren();
                options.NodeNames.AddRange(nodes.Select(n => n.Key));
            });

        var dynConfigProvider = new CenterConfigurationProvider();
        _services.AddSingleton(dynConfigProvider);
        ((IConfigurationBuilder) _configurationManager).Add(new CenterConfigurationProviderSource(dynConfigProvider));
        
        _services.AddOptions<CentersOptions>()
            .Bind(_configurationManager)
            .ValidateDataAnnotations();
        
        // Network options
        _services.AddOptions<CenterNetworkOptions>()
            .Bind(_configurationManager.GetSection("Network"))
            .GetOrganizationOptionsBuilder(_configurationManager)
            .BindOrganizationConfiguration("Network");
        
        return this;
    }
    
    
}