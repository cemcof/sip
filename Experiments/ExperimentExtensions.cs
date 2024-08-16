using sip.Core;
using sip.Scheduling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using sip.Experiments.Logs;
using sip.Experiments.Model;
using sip.Experiments.RemoteFilesystem;
using sip.Experiments.Samples;
using sip.Experiments.Workflows;
using sip.Organizations.Centers;

namespace sip.Experiments;

public class ExperimentsBuilder
{
    private readonly IServiceCollection _services;
    private readonly IConfiguration _configurationRoot;

    public ExperimentsBuilder(IServiceCollection services, IConfigurationRoot configurationRoot)
    {
        _services = services;
        _configurationRoot = configurationRoot;

        services.AddScheduledService<ExperimentEngine>(c =>
        {
            // Daily cron
            c.CronString = "0 0 1 * * *";
            c.Enabled = true;
        });
        
        services.AddScheduledService<ExperimentsDailyRoutine>(c =>
        {
            c.CronString = "0 0 1 * * *";
            c.Enabled = true;
        });
        
        services.AddSingleton<IExperimentHandler>(s => s.GetRequiredService<ExperimentEngine>());
        services.AddSingleton<IExperimentLogProvider>(s => s.GetRequiredService<ExperimentsService>());
        services.AddSingleton<ExperimentsService>();

        services.AddSingleton<IWorkflowProvider, FromConfigWorkflowProvider>();
        services.AddSingleton<IWorkflowProvider, WorkflowhubWorkflowProvider>();
        services.AddOptions<WorkflowhubOptions>()
            .GetOrganizationOptionsBuilder(configurationRoot)
            .BindOrganizationConfiguration("Workflowhub");
        
        services.AddOptions<List<Workflow>>()
            .GetOrganizationOptionsBuilder(configurationRoot)
            .BindOrganizationConfiguration("Workflows");

        services.AddOptions<EnginesOptions>()
            .GetOrganizationOptionsBuilder(configurationRoot)
            .BindOrganizationConfiguration("Engines");
        
        services.AddSingleton<IWorkflowProvider, CompositeWorkflowProvider>();
        
        services.AddScheduledService<CenterManager>(c =>
        {
            c.CronString = "*/5 * * * * *";
            c.Enabled = true;
            c.InitRun = true;
        });
        
        
        // services.AddSingleton<IInstrumentProvider>(s => s.GetRequiredService<CenterManager>());

        

        services.AddSingleton<SampleRepo>();
        
        
        services.AddScheduledService<RemoteFilesystemApiService>(c => c.Interval = TimeSpan.FromSeconds(2));
        services.AddSingleton<IFilesystemProvider>(s => s.GetRequiredService<RemoteFilesystemApiService>());
            
        services.Configure<SampleOptions>(configurationRoot.GetSection("Samples"));
        // services.AddEntityLogger<ExperimentLoggingProvider>(o => { });
        
        // Database
        services.ConfigureDbModel(modelBuilder =>
        {
            modelBuilder.Entity<EProject>();
            modelBuilder.Entity<Log>();
            modelBuilder.Entity<Sample>();

            // TimeSpans to strings
            modelBuilder.Entity<ExperimentStorage>()
                .Property(e => e.ExpirationPeriod)
                .HasConversion<string>();

            modelBuilder.Entity<ExperimentPublication>()
                .Property(p => p.EmbargoPeriod)
                .HasConversion<string>();
            
            var experiment = modelBuilder.Entity<Experiment>();
            
            experiment.HasOne(e => e.Operator)
                .WithMany()
                .HasForeignKey(e => e.OperatorId);
            experiment
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId);
            
            experiment.HasOne(e => e.Storage)
                .WithOne(ed => ed.Experiment)
                .HasForeignKey<ExperimentStorage>(e => e.ExperimentId);
            experiment.HasOne(e => e.Publication)
                .WithOne(ed => ed.Experiment)
                .HasForeignKey<ExperimentPublication>(e => e.ExperimentId);
            experiment.HasOne(e => e.Processing)
                .WithOne(ed => ed.Experiment)
                .HasForeignKey<ExperimentProcessing>(e => e.ExperimentId);

            modelBuilder.Entity<Log>()
                .Property(u => u.Level)
                .HasConversion(new EnumToStringConverter<LogLevel>())
                .HasMaxLength(256);
            
            // Explicitly set the relation between Experiment and Log
            // This is needed because nullable foreign keys are not cascade delted by default
            modelBuilder.Entity<Log>()
                .HasOne<Experiment>(l => l.Experiment)
                .WithMany(d => d.Logs)
                .HasForeignKey(k => k.ExperimentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Instrument authorization
        
        // Register handlers 
        _services.AddSingleton<InstrumentRemoteConnectAuthorizationHandler>();
        _services.AddSingleton<IAuthorizationHandler, InstrumentRemoteConnectAuthorizationHandler>(s => s.GetRequiredService<InstrumentRemoteConnectAuthorizationHandler>());
        _services.AddSingleton<IAuthorizationHandler, InstrumentJobsUseHandler>();
        // _services.AddOptions<AuthorizationOptions>()
        //     .Configure<IOptions<List<ExperimentOptions>>>((c, eopts) =>
        //     {
        //         // Add policies for all instruments of configured experiments 
        //         var insts = eopts.Value.Select(o => o.InstrumentName).Distinct();
        //         foreach (var inst in insts)
        //         {
        //             c.AddPolicy($"Jobs/{inst}", pb =>
        //             {
        //                 pb.AddRequirements(new IntrumentJobsUseRequirement(inst));
        //             });
        //             
        //             c.AddPolicy($"JobsFullcontrol/{inst}", pb =>
        //             {
        //                 pb.AddRequirements(new IntrumentJobsUseRequirement(inst, fullControl:true));
        //             });
        //             
        //             c.AddPolicy($"RemoteAccess/{inst}", pb =>
        //             {
        //                 pb.AddRequirements(new InstrumentRemoteDesktopRequirement(inst));
        //             });
        //         }
        //     });
        
        
        // Instruments config
        _services.AddOptions<InstrumentsOptions>()
            .GetOrganizationOptionsBuilder(configurationRoot)
            .Configure((options, configuration, organization) =>
            {
                foreach (var cs in configuration.GetSection("Instruments")
                             .GetChildren())
                {
                    options.Instruments.Add(new InstrumentRecord(
                        cs.GetValue<Guid>("Id"),
                        cs.GetValue<string>("Name")!,
                        cs.GetValue<string>("Alias")!,
                        organization,
                        cs.GetValue<string>("DisplayName")!,
                        cs.GetValue<string>("DisplayTheme")!
                        ));
                }
            });
        
        _services.AddOptions<ExperimentsOptions>()
            .GetOrganizationOptionsBuilder(configurationRoot)
            .ConfigureWithOptionsDependency<InstrumentsOptions>((options, conf, instrumentsOptions) =>
            {
                // TODO - refactor, will be used again for network instrument config
                foreach (var expConfig in conf.GetSection("Experiments")
                             .GetChildren())
                {
                    var inst = instrumentsOptions.Instruments.First(i => i.Name == expConfig.Key);
                    var expOptions = new Dictionary<string, ExperimentOptions>();
                    expConfig.Bind(expOptions);
                    options.InstrumentJobs[inst] = expOptions;
                }
            });
    }

    
}
