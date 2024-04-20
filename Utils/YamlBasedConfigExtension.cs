
namespace sip.Utils;

public static class YamlBasedConfigExtension
{
    /// <summary>
    /// Configures app's configuration sources, so they are:
    /// - appsettings YAML file selected by environment (e.g. appsettings.Development.yml), this file is compulsory!
    /// - Environment variables
    /// - Command line arguments
    /// </summary>
    public static void ConfigDefaultSourcesPlusYaml(this WebApplicationBuilder builder, string[] args)
    {
        var conf = builder.Configuration;
        
        // Obtain appsettings file name, can be possibly given as env variable or command line argument
        // If not given, use default: appsettings.{Environment}.yml
        conf.AddCommandLine(args); // Env provider is already added by default
        var appsettingsFile = conf.GetValue<string>(
            "appsettings",
            $"appsettings.{builder.Environment.EnvironmentName}.yml"
            )!;
        
        conf.Sources.Clear();
        conf.AddYamlFile(appsettingsFile, false, true);
        conf.AddEnvironmentVariables("ASPNETCORE_");
        conf.AddCommandLine(args);
    }
}