using Microsoft.Extensions.DependencyInjection.Extensions;

namespace sip.Documents.Renderers.Handlebars;

public class HandlebarsBuilder
{
    private readonly IServiceCollection _services;
    
    public HandlebarsBuilder(IServiceCollection services)
    {
        _services = services;
        _services.TryAddSingleton<HandlebarsService>();
        _services.TryAddSingleton<IHandlebarsService>(s => s.GetRequiredService<HandlebarsService>());

        AddInlineSingletonHelper<HandlebarsUrlHelper>();
        AddInlineSingletonHelper<HandlebarsDateStandardHelper>();
        AddInlineSingletonHelper<HandelbarsListHelpper>();
        AddInlineSingletonHelper<HandlebarsUrlBuilderHelper>();
        AddInlineSingletonHelper<HandlebarsDateHelper>();
    }

    public HandlebarsBuilder AddInlineSingletonHelper<THelper>() 
        where THelper : class, IHandlebarsInlineHelper
    {
        _services.AddSingleton<IHandlebarsInlineHelper, THelper>();
        return this;
    }
}

public static class HandlebarsExtensions;