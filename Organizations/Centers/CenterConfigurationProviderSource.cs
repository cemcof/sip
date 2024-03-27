
namespace sip.Organizations.Centers;

public class CenterConfigurationProviderSource
    (CenterConfigurationProvider dynConfigProvider) : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return dynConfigProvider;
    }
}