using System.Text.Json;

namespace sip.Organizations.Centers;

public class CenterConfigurationProvider : ConfigurationProvider
{
    public void LoadCenter(string organizationId, JsonElement configuration)
    {
        // Parse Json into configuration dictionary structure
        var data = JsonConfigParser.Parse(configuration);
        
        // Prefix dictionary case with organization name, which scopes the configuration to the organization 
        Data = data.ToDictionary(
            x => ConfigurationPath.Combine(organizationId, x.Key), 
            x => x.Value
            );
        
        // Notify this configuration was reloaded
        OnReload();
    }
}