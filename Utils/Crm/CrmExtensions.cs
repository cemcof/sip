using sip.Utils.NtlmRequest;

namespace sip.Utils.Crm;

public static class CrmExtensions
{
    public static void AddCrm(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<CrmOptions>()
            .Bind(config.GetSection("CRM"))
            .ValidateDataAnnotations();
        

        services.AddSingleton<HttpNtlmPyWrapper>();
        services.AddOptions<HttpNtlmPyOptions>()
            .Bind(config.GetSection("HttpNtlmPyWrapper"))
            .ValidateDataAnnotations();
        
        services.AddSingleton<CrmService>();
        
        // services.AddHttpClient<CrmService>((sp, httpc) =>
        //     {
        //         var opts = sp.GetRequiredService<IOptions<CrmOptions>>().Value;
        //         httpc.BaseAddress = opts.BaseUrl;
        //         httpc.DefaultRequestHeaders.Add("Prefer", "return=representation");
        //         // TODO - httpc.DefaultRequestHeaders.Accept = 
        //     })
        //     .ConfigurePrimaryHttpMessageHandler(sp =>
        //     {
        //         var opts = sp.GetRequiredService<IOptions<CrmOptions>>().Value;
        //         
        //         // TODO - this ? 
        //         CredentialCache credentials = new CredentialCache {
        //         {
        //             opts.BaseUrl, 
        //             "NTLM", 
        //             new NetworkCredential(opts.Username, opts.Password)
        //         }};
        //         
        //         // TODO - or this?
        //         // var credentials =
        //         //     new NetworkCredential(opts.Username, opts.Password, opts.BaseUrl.ToString());
        //         // TODO - wtf is domain? How to set it ? 
        //         return new HttpClientHandler()
        //         {
        //             Credentials = credentials
        //         };
        //     });

    }
}