using sip.Scheduling;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection.Extensions;
using sip.Core;
using sip.Projects.Statuses;

namespace sip.Projects;

public class ProjectBuilder<TProject>(IServiceCollection services)
    where TProject : Project
{
    public ProjectBuilder<TProject> WithIdGenerator<TIdGenerator, TOptions>(Action<TOptions>? configure = null) 
        where TIdGenerator : class, IIdGenerator<TProject>
        where TOptions : class
    {
        configure ??= _ => { };
        services.Configure(configure);
        services.AddSingleton<IIdGenerator<TProject>, TIdGenerator>();
        return this;
    }
}

public class ProjectsBuilder 
{
    private readonly IServiceCollection _services;
    private readonly IConfiguration _configuration;

    public ProjectsBuilder(IServiceCollection services, IConfiguration configuration)
    {
        _services = services;
        _configuration = configuration;

        services.AddSingleton<IProjectLoader, ProjectLoadingDelegator>();
        services.TryAddSingleton<ProjectManipulationHelperService>();
        services.TryAddSingleton<ProjectStatusHelperService>();
        services.TryAddSingleton<YearOrderIdGeneratorService>();
        services.AddSingleton<IProjectsFilterProvider, ProjectsFilterProvider>();
        services.AddSingleton<IProjectItemRenderProvider, ProjectRenderProviderDelegatorService>();

        new MessagingBuilder(services).AddProjectMessaging();

        services.ConfigureDbModel(mb =>
        {
            mb.Entity<Organization>();
            mb.Entity<Project>();
            mb.Entity<ProjectMember>();
            mb.Entity<Status>();
            mb.Entity<StatusInfo>().Property(s => s.Id); // ID has not setter therefore map it explicitly
            
        });
        
        // Status seeding from options 
        // Organizations seeding (from named options)
        _services.AddOptions<DbSeedOptions>()
            .Configure<IOptionListProvider<StatusOptions>>((seed, statopts) =>
            {
                var statusInfos = statopts.GetAll().Select(x => x.StatusDetails);
                seed.SeedEntities.AddRange(statusInfos);
            });

        var autorunner = _configuration.GetSection("ProjectPeriodicWorkflowRunner");
        if (autorunner.Exists())
        {
            services.AddScheduledService<ProjectPeriodicWorkflowRunner>(autorunner);
        }
        
    }

    public ProjectBuilder<TProject> AddProject<TProject>(Action<EntityTypeBuilder<TProject>>? modelConfig = null) where TProject : Project
    {
        // Add necessary defaults which are not required to be explicitly created by user
        _services.AddSingleton<IProjectItemRenderProvider<TProject>, DefaultProjectItemRenderProvider<TProject>>();
        // Discover entity model configuration
        
        
        modelConfig ??= _ => { };
        
        
        _services.ConfigureDbModel(builder => modelConfig(builder.Entity<TProject>()));
        var pbuilder = new ProjectBuilder<TProject>(_services);
        
        return pbuilder;
    }

    public ProjectsBuilder AddStatus<TStatusRef>() where TStatusRef : StatusDefinition
    {
        _services.ConfigureFromNamedSetup<StatusOptions, TStatusRef>();
        return this;
    }

    public ProjectsBuilder AutoAddProject<TProject>() where  TProject : Project
    {
        AddProject<TProject>();
        
        // Now automatically add implementations of several interfaces
        var projectDefinedInterfaces = new List<Type>()
        {
            typeof(IProjectDefine<TProject>),
            typeof(IProjectItemRenderProvider<TProject>),
            typeof(IProjectFactory<TProject>),
            typeof(IProjectLoader<TProject>),
            typeof(IProjectDailyActionHandler<TProject>),
            typeof(IProjectMessaging<TProject>),
            typeof(IIdGenerator<TProject>),
        };
        
        _services.Scan(s =>
        {
            s.FromCallingAssembly()
                .AddClasses(c => 
                    c.Where(t => projectDefinedInterfaces.Any(i => i.IsAssignableFrom(t)))
                    )
                .AsSelfWithInterfaces()
                .WithSingletonLifetime();
        });

        return this;
    }

    public ProjectsBuilder AutoScanProjectTypes(Assembly assembly)
    {
        var projectTypes = assembly.FindImplementations<Project>(false);
        foreach (var projectType in projectTypes)
        {
            var meth = GetType().GetMethod(nameof(AutoAddProject))!.MakeGenericMethod(projectType);
            meth.Invoke(this, null);
        }

        return this;
    }

    public ProjectsBuilder AutoScanStatuses(Assembly assembly)
    {   
        var statusRefTypes = assembly.FindImplementations<StatusDefinition>(includeSelf:false);
        foreach (var statusRefType in statusRefTypes)
        {
            var meth = GetType().GetMethod(nameof(AddStatus))!.MakeGenericMethod(statusRefType);
            meth.Invoke(this, null);
        }

        return this;
    }
}