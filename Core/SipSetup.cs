using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using sip.Auth.Verification;
using sip.CEITEC.CIISB;
using sip.CEITEC.DirectAccess;
using sip.Core.IndexRedirecting;

namespace sip.Core;

public class SipSetup(string[] args)
{
    public void AutoDefault(Action<SipBuilder> configure)
    {
        var assebmly = Assembly.GetExecutingAssembly();
        var wb = WebApplication.CreateBuilder(args);
        
        var projmanBuilder = new SipBuilder(wb);
        Action<AppOptions>? configureApp = null;
        // ============================================================================
        // Configure base, configuration and logging 
        // ============================================================================
        // Create logger factory that can be used during startup
        var loggerFac = LoggerFactory.Create(o =>
        {
            o.SetMinimumLevel(LogLevel.Debug);
            o.AddConsole();
        });
        
        wb.ConfigDefaultSourcesPlusYaml(args);
        
        var conf1 = wb.Configuration;
        configureApp ??= _ => { };
        var hostAssembly = Assembly.GetCallingAssembly();
        var hostAssemblyName = hostAssembly.GetName().Name;
        // Support better options monitor
        // Data directory validator
        // DELME - not need this probably 
        // bool DataDirValidator(AppOptions o)
        // {
        //     try
        //     {
        //         var dir = Directory.CreateDirectory(o.DataDirectory);
        //         File.WriteAllText(Path.Combine(dir.FullName, "DELME.tmp"), string.Empty);
        //         return true;
        //     }
        //     catch (Exception e)
        //     {
        //         Console.WriteLine(e);
        //         return false;
        //     }
        // }
    
        // This seems to be necessary for other environments than Development
        wb.WebHost.UseStaticWebAssets();
        
        // Configure basic application options
        wb.Services.AddOptions<AppOptions>() 
            .Configure(o => o.HostAppAssembly = hostAssembly)
            .Bind(conf1.GetSection("App"))
            .Configure(configureApp)
            // Check that data directory exists and is writable
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        var se = wb.Services;
        se.AddSingleton(TimeProvider.System);
        // =================================================================
        // Configure database, identity and authentication
        // =================================================================
        se.AddDbContext<AppDbContext>(o =>
        {
            var constring = conf1.GetSection("Db")
                .GetValue<string>("ConnectionString") ?? throw new ArgumentException("Missing Db:ConnectionString");
            o.UseNpgsql(constring, optionsBuilder =>
            {
                optionsBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });
            // TODO - not for production
            o.EnableSensitiveDataLogging();
            o.EnableDetailedErrors();
        }, optionsLifetime: ServiceLifetime.Singleton, contextLifetime: ServiceLifetime.Scoped);
        
        se.AddDbContextFactory<AppDbContext>();
        se.AddSingleton<IEntityMerger<AppDbContext>, RawSqlEntityMerger<AppDbContext>>();
        se.AddOptions<EntityMergerOptions>().Bind(conf1.GetSection("Db"));
        se.AddSingleton<IDbSeeder, DbSeeder>();
        se.Configure<DbSeedOptions>(conf1.GetSection("Db:Init"));
        se.AddHttpClient();
        se.ConfigureDbModel(cf => cf.ApplyConfigurationsFromAssembly(hostAssembly)); 
        // =================================================================
        // Configure necessary framework core services 
        // =================================================================
        se.AddControllersWithViews(o =>
            {
                // We dont want to use newtonsoft json for all, but we need it just for jsonpatch
                // Official workaround from https://learn.microsoft.com/en-us/aspnet/core/web-api/jsonpatch?view=aspnetcore-8.0
                var builder = new ServiceCollection()
                    .AddLogging()
                    .AddMvc()
                    .AddNewtonsoftJson()
                    .Services.BuildServiceProvider();

                var formatter = builder
                    .GetRequiredService<IOptions<MvcOptions>>()
                    .Value
                    .InputFormatters
                    .OfType<NewtonsoftJsonPatchInputFormatter>()
                    .First();

                o.InputFormatters.Insert(0, formatter);
            })
            .AddJsonOptions(o => {
                o.JsonSerializerOptions.PropertyNamingPolicy = null; 
                o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
        
        se.AddServerSideBlazor();
        
        
     
        se.AddSingleton(typeof(IOptionListProvider<>), typeof(OptionListProvider<>));
        // Main layout / gui
        // var guiBuilder = new MainGuiBuilder(se);
        // guiBuilder.Options.Bind(conf.GetSection("Gui"));
        
        se.Configure<AuthOptions>(conf1.GetSection("Authentication"));

        
        se.AddAuthorization(o =>
        {
            // Add policy with no requirement, added verifiers will overwrite it
            o.AddPolicy(IVerificator.USER_VERIFICATION_POLICY, p => p.AddRequirements(new DummyAuthRequirement()));
        });
        
        // se.AddTransient<IAuthorizationPolicyProvider, PolicyProvider>();
        se.AddSingleton<IAuthorizationHandler, RoleNetworkAuthorizationHandler>();
        se.AddSingleton<IAuthorizationHandler, NotIpBlacklistedHandler>();
        se.AddSingleton<IAuthorizationHandler, AppUserRolesRequirementHandler>();
        se.AddSingleton<IAuthorizationHandler, DummyAuthHandler>();
        se.AddSingleton<IAuthorizationHandler, NotUserAppAuthenticatedRequirement>();
        se.AddSingleton<IAuthorizationHandler, NotMatchesNoSignInNetworkHandler>();
        
        // In memory cache
        se.AddMemoryCache();
        var confLoggerFactory = loggerFac;
     
        var (services, conf) = (wb.Services, wb.Configuration);
            
        var usermanBuilder = new UsermanBuilder(services, confLoggerFactory.CreateLogger<UsermanBuilder>())
            .AddUserManagement()
            .ScanForRoleRefs(assebmly);
        
        
        var projectsBuilder = new ProjectsBuilder(services, conf);
        // projectsBuilder.AutoScanProjectTypes(assebmly); E Project errors
        projectsBuilder.AutoAddProject<CProject>();
        projectsBuilder.AutoAddProject<DProject>();
        
        projectsBuilder.AutoScanStatuses(assebmly);

        var organizationsBuilder = new OrganizationBuilder(services, confLoggerFactory.CreateLogger<OrganizationBuilder>()
        , wb.Configuration);
        organizationsBuilder.ScanForOrganizations(assebmly);
        organizationsBuilder.AddCenters();

        var documentsBuilder = new DocumentsBuilder(services, conf);
        documentsBuilder.AutoScanDocuments(assebmly);
        
        var messagingBuilder = new MessagingBuilder(services);
        messagingBuilder
            .AddOutEmailMessaging(conf.GetSection("Messaging:Smtp"));
        // .AddInEmailMessaging(conf.GetSection("Messaging:Imap"));

        var authenticationBuilder = services.AddAuthentication()
            .UseImpersonation(wb.Environment)
            .AddSaml2(conf.GetSection("Authentication:External:Eduid"))
            .AddGoogle(conf.GetSection("Authentication:External:Google"))
            .AddOrcid(conf.GetSection("Authentication:External:Orcid"))
            .AddTokenAuth(conf.GetSection("Authentication:Jwt"));

        // Setup gui
        se.AddSingleton<IIndexRedirector, DummyIndexRedirector>();
        se.AddOptions<MainGuiOptions>()
            .Bind(conf.GetSection("Gui"));
        
        configure(projmanBuilder);
        
        var app = wb.Build();
        
        
        AutoPipelineDefault(app);
        app.Run();
    }

    public WebApplication AutoPipelineDefault(WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseMiddleware<JwtTokenMiddleware>();
        app.UseAuthentication();
        app.UseMiddleware<NetworkAddressAuthenticationMiddleware>();
        app.UseAuthorization();

        // if (useWebsockets)
        // {
        //     app.UseWebSockets();
        // }

        // Map routes
        app.MapControllers();
        app.MapBlazorHub();
        app.MapFallbackToController(nameof(MainController.Host), "Main");

        // DEBUG
        // app.Lifetime.ApplicationStarted.Register(() => Console.WriteLine("! Application is starting"));
        // app.Lifetime.ApplicationStopping.Register(() => Console.WriteLine("! Application is stopping"));
        // app.Lifetime.ApplicationStopped.Register(() => Console.WriteLine("! Application is stopped"));
        //
        #region POST CONFIG
        // Run configured delegates
        // For web app
        var delegates = app.Services.GetRequiredService<IOptions<List<WebAppDelegate>>>();
        foreach (var webAppDelegate in delegates.Value)
        {
            webAppDelegate(app);
        }

        // Db seeding
        var dbInitializer = app.Services.GetRequiredService<IDbSeeder>();
        dbInitializer.SimpleDbSeed();
        #endregion

        return app;
    }
}

public class SipBuilder(WebApplicationBuilder webApplicationBuilder)
{
    public WebApplicationBuilder WebApplicationBuilder { get; } = webApplicationBuilder;
}