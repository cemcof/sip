using System.Diagnostics;

namespace sip.Organizations;
public static class OrganizationExtensions
{
    /// <summary>
    /// Gets options scoped to given organization.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="forOrganization"></param>
    /// <typeparam name="TOptions"></typeparam>
    /// <returns></returns>
    public static TOptions Get<TOptions>(this IOptionsMonitor<TOptions> options, IOrganization forOrganization)
        => options.Get(forOrganization.Id);
    
    
    public static OrganizationOptionsBuilder<TOptions> GetOrganizationOptionsBuilder<TOptions>(this OptionsBuilder<TOptions> ob)
        where TOptions : class
    {
        return new OrganizationOptionsBuilder<TOptions>(ob);
    }
    
}

public class ConfigureOrganizationOptionsFromConfiguration<TOptions> 
    (IConfigurationRoot configuration, string subsection) : IConfigureNamedOptions<TOptions> where TOptions : class
{
    public void Configure(string? name, TOptions options)
    {
        if (string.IsNullOrWhiteSpace(name)) return;

        var targetSection = configuration.GetSection(ConfigurationPath.Combine(name, subsection));
        targetSection.Bind(options);
    }

    public void Configure(TOptions options)
    {
        // No organization given, noop
    }
}

public class ConfigureOrganizationOptionsFromActionWithOptionsDependency<TOptions, TDep1>(
        IOptionsMonitor<TDep1> optsMonitor, 
        Action<TOptions, IConfiguration, TDep1> action,
        IConfiguration configuration
        )
    : IConfigureNamedOptions<TOptions>
    where TOptions : class
    where TDep1 : class
{
    public void Configure(TOptions options)
    {
        // No organization, noop
    }

    public void Configure(string? name, TOptions options)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        
        var optsDep1 = optsMonitor.Get(name);
        if (configuration is not IConfigurationRoot)
            throw new InvalidOperationException("Configuration must be IConfigurationRoot");

        var orgSection = configuration.GetSection(name);
        
        action(options, orgSection, optsDep1);
    }
}

public class ConfigureOrganizationFromAction<TOptions>(
    IConfigurationRoot configurationRoot, 
    IOrganizationProvider organizationProvider,
    Action<TOptions, IConfiguration, IOrganization> configure
    ) : IConfigureNamedOptions<TOptions>
    where TOptions : class
{
    public void Configure(TOptions options)
    {
        // No organization - noop
    }

    public void Configure(string? name, TOptions options)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        
        var org = organizationProvider.GetFromStringOrDefault(name);
        if (org is null)
        {
            // We are trying to configure organization that does not exist
            // Noop
            return;
        }
        
        var conf = configurationRoot.GetSection(org.Id);
        configure(options, conf, org);
    }
}

public class OrganizationOptionsBuilder<TOptions>(OptionsBuilder<TOptions> optionsBuilder)
    where TOptions : class
{
    public OrganizationOptionsBuilder<TOptions> BindOrganizationConfiguration(IConfigurationRoot configuration, string subsection)
    {
        optionsBuilder.Services.AddSingleton<IOptionsChangeTokenSource<TOptions>>(new ConfigurationChangeTokenSource<TOptions>(null, configuration));
        optionsBuilder.Services.AddSingleton<IConfigureOptions<TOptions>>(
            new ConfigureOrganizationOptionsFromConfiguration<TOptions>(configuration, subsection)
            );
        
        return this;
    }

    public OrganizationOptionsBuilder<TOptions> ConfigureWithOptionsDependency<TDep1>(Action<TOptions, IConfiguration, TDep1> action)
        where TDep1 : class
    {
        optionsBuilder.Services.AddSingleton<IConfigureOptions<TOptions>>(s =>
        {
            var dep1 = s.GetRequiredService<IOptionsMonitor<TDep1>>();
            var config = s.GetRequiredService<IConfiguration>();
            Debug.Assert(config is IConfigurationRoot);
            return new ConfigureOrganizationOptionsFromActionWithOptionsDependency<TOptions, TDep1>(dep1, action, config);
        });

        return this;
    }

    public OrganizationOptionsBuilder<TOptions> Configure(Action<TOptions, IConfiguration, IOrganization> configure)
    {
        optionsBuilder.Services.AddSingleton<IConfigureOptions<TOptions>>(s =>
        {
            var orgProvider = s.GetRequiredService<IOrganizationProvider>();
            var config = s.GetRequiredService<IConfiguration>(); 
            Debug.Assert(config is IConfigurationRoot);
            return new ConfigureOrganizationFromAction<TOptions>((IConfigurationRoot)config, orgProvider, configure);
        });

        return this;
    }


}