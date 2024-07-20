using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace sip.Auth.Verification.Contact;

public static class ContactVerificationExtensions
{
     
    public static AuthenticationBuilder AddContactVerification(this AuthenticationBuilder builder, 
        Action<ContactVerifierOptions>? configureOptions = null)
    {
        configureOptions ??= _ => { };
        builder.Services.Configure(configureOptions);
        builder.Services.AddSingleton<ContactVerifierService>();
        builder.Services.AddSingleton<IAuthorizationHandler>(s => s.GetRequiredService<ContactVerifierService>());
        builder.Services.AddSingleton<IVerificator>(s => s.GetRequiredService<ContactVerifierService>());
        
        // Plug into authorization framework
        builder.Services.AddAuthorization(a =>
        {
            a.AddPolicy(IVerificator.USER_VERIFICATION_POLICY, pb =>
            {
                var prevPolicy = a.GetPolicy(IVerificator.USER_VERIFICATION_POLICY);
                if (prevPolicy != null) pb.Combine(prevPolicy);
                pb.AddRequirements(new ContactVerifiedRequirement());
            });
            
        });
            
        return builder;
    }
    
    public static AuthenticationBuilder AddContactVerification(this AuthenticationBuilder builder, IConfigurationSection config)
    {
        AddContactVerification(builder);
        builder.Services.AddOptions<ContactVerifierOptions>().Bind(config);
        return builder;
    }
}